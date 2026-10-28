namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;

public class CreatePersonTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;

    public CreatePersonTests(TestDbContextFixture fixture)
    {
        _fixture = new TestDbContextFixture();
    }

    [Fact]
    public async Task Process_ThrowsException_WhenNameIsEmpty()
    {
        // Arrange
        var command = new CreatePerson
        {
            Name = ""
        };
        var preProcessor = new CreatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Process_ThrowsException_WhenNameIsWhitespace()
    {
        // Arrange
        var command = new CreatePerson
        {
            Name = "   "
        };
        var preProcessor = new CreatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_ThrowsException_WhenPersonAlreadyExists()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new CreatePerson
        {
            Name = existingPerson.Name
        };
        var preProcessor = new CreatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_DoesNotThrowException_WhenNameIsProvided()
    {
        // Arrange
        var command = new CreatePerson
        {
            Name = "John Doe"
        };
        var preProcessor = new CreatePersonPreProcessor(_fixture.Context);

        // Act
        var ex = await Record.ExceptionAsync(() => preProcessor.Process(command, CancellationToken.None));

        // Assert
        Assert.Null(ex);
    }


    [Fact]
    public async Task Handle_CreatesPersonAndReturnsId()
    {
        // Arrange
        var command = new CreatePerson
        {
            Name = "John Doe"
        };
        var handler = new CreatePersonHandler(_fixture.Context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.True(result.Id > 0);

        var createdPerson = await _fixture.Context.People.FindAsync(result.Id);
        Assert.NotNull(createdPerson);
        Assert.Equal("John Doe", createdPerson.Name);
    }
}