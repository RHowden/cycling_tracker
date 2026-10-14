using System.Net;
using System.Net.Http.Json;
using CyclingTracker.Api.Controllers;
using CyclingTracker.Api.IntegrationTests.Infrastructure;
using CyclingTracker.Api.Services.SessionQuery;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;

namespace CyclingTracker.Api.IntegrationTests.Features;

public class ControllerStrutureTests : IClassFixture<IntegrationFactory>
{
    private readonly HttpClient _client;
    Mock<ISessionQueryService> _queryMock = new();
    private readonly WebApplicationFactory<Program> _factory;

    public ControllerStrutureTests(IntegrationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<ISessionQueryService>();
                services.AddScoped(_ => _queryMock.Object);
            }));

        _client = _factory.CreateClient();
    }

    [Fact]
    public void Every_controller_declares_its_authorization()
    {
        var missing = typeof(ImportController).Assembly.GetTypes()
            .Where(t => typeof(ControllerBase).IsAssignableFrom(t) && !t.IsAbstract)
            .Where(t => !t.IsDefined(typeof(AuthorizeAttribute), inherit: true)
                     && !t.IsDefined(typeof(AllowAnonymousAttribute), inherit: true))
            .Select(t => t.Name);

        Assert.Empty(missing);
    }

    [Theory]
    [InlineData("not-a-guid")]
    [InlineData("00000000-0000-0000-0000-000000000000")]
    public async Task GetSessions_UserIdClaimInvalid_Returns401Problem_AndSkipsService(string badUserId)
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/api/sessions");
        request.Headers.Add("X-Test-UserId", badUserId);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);

        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        Assert.NotNull(problem);
        Assert.Equal(401, problem.Status);

        _queryMock.Verify(s => s.GetUserSessionsAsync(It.IsAny<Guid>()), Times.Never);
    }
}