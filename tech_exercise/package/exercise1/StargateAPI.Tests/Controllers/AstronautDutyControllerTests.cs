namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using Moq;
using MediatR;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

public class AstronautDutyControllerTests : IClassFixture<TestDbContextFixture>
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly AstronautDutyController _controller;

    public AstronautDutyControllerTests(TestDbContextFixture fixture)
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new AstronautDutyController(_mediatorMock.Object);
    }

    public async Task GetAstronautDutiesByName_ReturnsOkResult_WhenMediatorReturnsSuccess()
    {
        // Arrange
        var name = "John Doe";
        var response = new GetAstronautDutiesByNameResult
        {
            Success = true,
            Message = "Success",
            ResponseCode = 200
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.GetAstronautDutiesByName(name);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<GetAstronautDutiesByNameResult>(okResult.Value);
        Assert.True(returnedResponse.Success);
    }

    public async Task CreateAstronautDuty_ReturnsOkResult_WhenMediatorReturnsSuccess()
    {
        // Arrange
        var request = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };

        var response = new CreateAstronautDutyResult
        {
            Success = true,
            Message = "Success",
            ResponseCode = 200
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.CreateAstronautDuty(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedResponse = Assert.IsType<CreateAstronautDutyResult>(okResult.Value);
        Assert.True(returnedResponse.Success);
    }

    public async Task GetAstronautDutiesByName_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var name = "John Doe";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetAstronautDutiesByName(name);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    public async Task GetAstronautDutiesByName_ReturnsBadRequest_WhenMediatorThrowsBadHttpRequestException()
    {
        // Arrange
        var name = "John Doe";

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetAstronautDutiesByName>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new BadHttpRequestException("Test bad request"));

        // Act
        var result = await _controller.GetAstronautDutiesByName(name);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }

    public async Task CreateAstronautDuty_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var request = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.CreateAstronautDuty(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status500InternalServerError, objectResult.StatusCode);
    }

    public async Task CreateAstronautDuty_ReturnsBadRequest_WhenMediatorThrowsBadHttpRequestException()
    {
        // Arrange
        var request = new CreateAstronautDuty
        {
            Name = "John Doe",
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreateAstronautDuty>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new BadHttpRequestException("Test bad request"));

        // Act
        var result = await _controller.CreateAstronautDuty(request);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status400BadRequest, objectResult.StatusCode);
    }   
}
