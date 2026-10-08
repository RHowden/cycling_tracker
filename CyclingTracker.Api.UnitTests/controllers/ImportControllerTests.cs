using System.Security.Claims;
using CyclingTracker.Api.Controllers;
using CyclingTracker.Api.Dtos;
using CyclingTracker.Api.Services;
using CyclingTracker.Api.Shared;
using CyclingTracker.Api.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;

public class ImportControllerTests
{
    private readonly Mock<IFitImportService> _importService = new();
    private readonly Guid _userID = Guid.NewGuid();

    // ---------- helpers ----------

    private ImportController CreateController(Guid? userId = null)
    {
        var nullSafeUserId = userId ?? _userID;

        var controller = new ImportController(_importService.Object);

        // Fake a logged-in user, since we skip the auth pipeline by calling the action directly
        var claims = new[] { new Claim(ClaimTypes.NameIdentifier, nullSafeUserId.ToString()) };
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext
            {
                User = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuth"))
            }
        };

        return controller;
    }

    private static IFormFile CreateFormFile(byte[] content, string fileName = "test.fit")
    {
        var stream = new MemoryStream(content);
        return new FormFile(stream, 0, content.Length, "file", fileName);
    }

    // ---------- tests ----------

    [Fact]
    public async Task Upload_ValidFile_ReturnsCreatedWithNewId()
    {
        // Arrange
        var expectedSessionID = Guid.NewGuid();
        _importService
            .Setup(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()))
            .ReturnsAsync(Result<SessionDto>.Success(new SessionDto(expectedSessionID)));

        var controller = CreateController();
        var file = CreateFormFile(new byte[] { 1, 2, 3 });

        // Act
        var result = await controller.Import(file);

        // Assert
        // 1. Check that the result is specifically a 201 CreatedActionResult
        var createdResult = Assert.IsAssignableFrom<CreatedAtActionResult>(result);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);

        // 2. Cast the correct ID is returned
        var returnedDto = Assert.IsAssignableFrom<SessionDto>(createdResult.Value);
        Assert.Equal(expectedSessionID, returnedDto.Id);
    }

    [Fact]
    public async Task Upload_NullFile_ReturnsBadRequest_AndDoesNotCallService()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = await controller.Import(null!);

        // Assert
        var createdResult = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, createdResult.StatusCode);

        _importService.Verify(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Upload_EmptyFile_ReturnsBadRequest_AndDoesNotCallService()
    {
        // Arrange
        var controller = CreateController();
        var file = CreateFormFile(new byte[] { });

        // Act
        var result = await controller.Import(file);

        // Assert
        var createdResult = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, createdResult.StatusCode);

        _importService.Verify(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()),
            Times.Never);
    }

    [Fact]
    public async Task Upload_FileWithInvalidExtension_ReturnsBadRequest_AndDoesNotCallService()
    {
        // Arrange
        var controller = CreateController();
        var file = CreateFormFile(new byte[] { }, "bad_extension.txt");

        // Act
        var result = await controller.Import(file);

        // Assert
        var createdResult = Assert.IsAssignableFrom<BadRequestObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, createdResult.StatusCode);

        _importService.Verify(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()),
            Times.Never);
    }

       [Fact]
    public async Task Upload_WithServiceError_ReturnsMessage()
    {
        // Arrange
        var error = "error occured";
        _importService
          .Setup(s => s.ImportAsync(It.IsAny<IFormFile>(), It.IsAny<Guid>()))
          .ReturnsAsync(Result<SessionDto>.Failure(ServiceErrorType.Failure, error));

        var controller = CreateController();
        var file = CreateFormFile(new byte[] { 1, 2, 3 });

        // Act
        var result = await controller.Import(file);

        // Assert
        var createdResult = Assert.IsAssignableFrom<UnprocessableEntityObjectResult>(result);
        Assert.Equal(StatusCodes.Status422UnprocessableEntity, createdResult.StatusCode);
    }

    [Fact]
    public async Task Upload_PassesAuthenticatedUserIdToService()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid(); // The specific user ID we expect to find

        // Tell our mock service to return a happy success result wrapper
        _importService
            .Setup(s => s.ImportAsync(It.IsAny<IFormFile>(), expectedUserId))
            .ReturnsAsync(Result<SessionDto>.Success(new SessionDto(new Guid())));

        var controller = CreateController(expectedUserId);
        var file = CreateFormFile(new byte[] { 1, 2, 3 });

        // Act
        var result = await controller.Import(file);

        // Assert
        _importService.Verify(
            s => s.ImportAsync(It.IsAny<IFormFile>(), expectedUserId),
            Times.Once);
    }
}