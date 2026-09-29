using System.Net;
using System.Net.Http.Headers;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Shared;
using Microsoft.AspNetCore.Http;
using Moq;

namespace CyclingTracker.Api.IntegrationTests;

public class ImportEndpointTests : IClassFixture<CustomWebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    private readonly CustomWebApplicationFactory<Program> _factory;

    public ImportEndpointTests(CustomWebApplicationFactory<Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task Import_ValidFile_Returns201Created_WithValidLocationHeader()
    {
        // Arrange
        var newSessionId = Guid.NewGuid();
        _factory.FitImportServiceMock
            .Setup(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result<SessionDto>.Success(new SessionDto(newSessionId, 32.4)));

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
