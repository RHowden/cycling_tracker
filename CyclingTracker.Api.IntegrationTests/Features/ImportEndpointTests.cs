using System.Net;
using System.Net.Http.Headers;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection.Extensions;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.IntegrationTests.Infrastructure;
using CyclingTracker.Api.Shared;
using Microsoft.AspNetCore.Http;
using Moq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc.Testing;
using CyclingTracker.Api.Services;

namespace CyclingTracker.Api.IntegrationTests.Features;

public class ImportEndpointTests : IClassFixture<IntegrationFactory>, IDisposable
{
    private readonly HttpClient _client;
    private readonly Mock<IFitImportService> _importMock = new();
    private readonly WebApplicationFactory<Program> _factory;

    public ImportEndpointTests(IntegrationFactory factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
            builder.ConfigureTestServices(services =>
            {
                services.RemoveAll<IFitImportService>();
                services.AddScoped(_ => _importMock.Object);
            }));

        _client = _factory.CreateClient();
    }

    public void Dispose()
    {
        _factory.Dispose();
        GC.SuppressFinalize(this);
    }

    [Fact]
    public async Task Import_ValidFile_Returns201Created_WithValidLocationHeader()
    {
        // Arrange
        var newSessionId = Guid.NewGuid();
        _importMock
            .Setup(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result<SessionDto>.Success(new SessionDto(newSessionId)));

        // Create actual HTTP multipart form content matching a browser upload
        using var content = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(new byte[] { 1, 2, 3 });
        fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
        content.Add(fileContent, "file", "workout.fit");

        // Act: Sends a real network-style call through the in-memory server
        var response = await _client.PostAsync("/api/import/import", content);

        // Assert
        // 🔥 IF YOUR ROUTING IS BROKEN, THIS LINE WILL CATCH IT! 
        // It will fail because the server will return a 500 Internal Server Error instead of 201.
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);

        // Verify the Location header was built correctly across controllers
        var locationHeader = response.Headers.Location;
        Assert.NotNull(locationHeader);
        Assert.Contains($"/api/Sessions/{newSessionId}", locationHeader.ToString());
    }
}
