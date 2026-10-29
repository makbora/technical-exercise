namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StargateAPI.Controllers;
using Moq;
using MediatR;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Queries;

public class PersonControllerTests : IClassFixture<TestDbContextFixture>
{
    private readonly Mock<IMediator> _mediatorMock;
    private readonly PersonController _controller;

    public PersonControllerTests(TestDbContextFixture fixture)
    {
        _mediatorMock = new Mock<IMediator>();
        _controller = new PersonController(_mediatorMock.Object);
    }

    [Fact]
    public async Task GetPeople_ReturnsOkResult_WhenMediatorReturnsSuccess()
    {
        // Arrange
        var response = new GetPeopleResult
        {
            Success = true,
            Message = "Success",
            ResponseCode = 200
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPeople>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.GetPeople();

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        var returnedResponse = Assert.IsType<GetPeopleResult>(okResult.Value);
        Assert.Equal(200, returnedResponse.ResponseCode);
        Assert.True(returnedResponse.Success);
    }

    [Fact]
    public async Task GetPeople_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPeople>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetPeople();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
        var returnedResponse = Assert.IsType<BaseResponse>(objectResult.Value);
        Assert.False(returnedResponse.Success);
        Assert.Equal("Test exception", returnedResponse.Message);
    }

    [Fact]
    public async Task GetPeople_ReturnsBadRequest_WhenMediatorThrowsBadHttpRequestException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPeople>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new BadHttpRequestException("Test bad request", StatusCodes.Status400BadRequest));

        // Act
        var result = await _controller.GetPeople();

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
        var returnedResponse = Assert.IsType<BaseResponse>(objectResult.Value);
        Assert.False(returnedResponse.Success);
        Assert.Equal("Test bad request", returnedResponse.Message);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsOkResult_WhenMediatorReturnsSuccess()
    {
        // Arrange
        var response = new GetPersonByNameResult
        {
            Success = true,
            Message = "Success",
            ResponseCode = 200
        };

        _mediatorMock.Setup(m => m.Send(It.Is<GetPersonByName>(q => q.Name == "John Doe"), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.GetPersonByName("John Doe");

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(200, okResult.StatusCode);
        Assert.Same(response, okResult.Value);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var name = "John Doe";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPersonByName>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.GetPersonByName(name);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task GetPersonByName_ReturnsBadRequest_WhenMediatorThrowsBadHttpRequestException()
    {
        // Arrange
        var name = "John Doe";
        _mediatorMock.Setup(m => m.Send(It.IsAny<GetPersonByName>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new BadHttpRequestException("Test bad request", StatusCodes.Status400BadRequest));

        // Act
        var result = await _controller.GetPersonByName(name);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreatePerson_ReturnsOkResult_WhenMediatorReturnsSuccess()
    {
        // Arrange
        var response = new CreatePersonResult
        {
            Success = true,
            Message = "Success",
            ResponseCode = 200
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePerson>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.CreatePerson("John Doe");

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        var returnedResponse = Assert.IsType<CreatePersonResult>(okResult.Value);
        Assert.Equal(200, returnedResponse.ResponseCode);
        Assert.True(returnedResponse.Success);
    }

    [Fact]
    public async Task CreatePerson_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePerson>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.CreatePerson("John Doe");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task CreatePerson_ReturnsBadRequest_WhenMediatorThrowsBadHttpRequestException()
    {
        // Arrange
        _mediatorMock.Setup(m => m.Send(It.IsAny<CreatePerson>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new BadHttpRequestException("Test bad request", StatusCodes.Status400BadRequest));

        // Act
        var result = await _controller.CreatePerson("John Doe");

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsOkResult_WhenMediatorReturnsSuccess()
    {
        // Arrange
        var command = new UpdatePerson
        {
            Name = "John Doe",
            NewName = "John D"
        };
        var response = new UpdatePersonResult
        {
            Success = true,
            Message = "Success",
            ResponseCode = 200
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePerson>(), It.IsAny<CancellationToken>()))
                     .ReturnsAsync(response);

        // Act
        var result = await _controller.UpdatePerson("John Doe", command);

        // Assert
        var okResult = Assert.IsType<ObjectResult>(result);
        var returnedResponse = Assert.IsType<UpdatePersonResult>(okResult.Value);
        Assert.Equal(200, returnedResponse.ResponseCode);
        Assert.True(returnedResponse.Success);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsInternalServerError_WhenMediatorThrowsException()
    {
        // Arrange
        var command = new UpdatePerson
        {
            Name = "John Doe",
            NewName = "John D"
        };
        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePerson>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new Exception("Test exception"));

        // Act
        var result = await _controller.UpdatePerson("John Doe", command);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, objectResult.StatusCode);
    }

    [Fact]
    public async Task UpdatePerson_ReturnsBadRequest_WhenMediatorThrowsBadHttpRequestException()
    {
        // Arrange
        var command = new UpdatePerson
        {
            Name = "John Doe",
            NewName = "John Doe"
        };

        _mediatorMock.Setup(m => m.Send(It.IsAny<UpdatePerson>(), It.IsAny<CancellationToken>()))
                     .ThrowsAsync(new BadHttpRequestException("Test bad request", StatusCodes.Status400BadRequest));

        // Act
        var result = await _controller.UpdatePerson("John Doe", command);

        // Assert
        var objectResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(400, objectResult.StatusCode);
    }
}

