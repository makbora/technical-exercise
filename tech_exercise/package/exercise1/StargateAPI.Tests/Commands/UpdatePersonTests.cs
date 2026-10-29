namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;

public class UpdatePersonTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;

    public UpdatePersonTests(TestDbContextFixture fixture)
    {
        _fixture = new TestDbContextFixture();
    }

    [Fact]
    public async Task Process_ThrowsException_WhenNewNameIsEmpty()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new UpdatePerson
        {
            Name = existingPerson.Name,
            NewName = ""
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }
    
    [Fact]
    public async Task Process_ThrowsException_WhenNewNameIsWhitespace()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new UpdatePerson
        {
            Name = existingPerson.Name,
            NewName = "   "
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_ThrowsException_WhenPersonDoesNotExist()
    {
        // Arrange
        var command = new UpdatePerson
        {
            Name = "Non-existent Person",
            NewName = "New Name"
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_ThrowsException_WhenNewNameAlreadyExists()
    {
        // Arrange
        var existingPerson1 = _fixture.CreateTestPerson("Existing Person 1");
        var existingPerson2 = _fixture.CreateTestPerson("Existing Person 2");
        var command = new UpdatePerson
        {
            Name = existingPerson1.Name,
            NewName = existingPerson2.Name
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_ThrowsException_WhenNewNameDiffersOnlyByCapitalizationFromAnotherPerson()
    {
        // Arrange
        var existingPerson1 = _fixture.CreateTestPerson("Existing Person 1");
        _fixture.CreateTestPerson("Existing Person 2");
        var command = new UpdatePerson
        {
            Name = existingPerson1.Name,
            NewName = "EXISTING person 2"
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_FindsPersonIgnoringCapitalization()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new UpdatePerson
        {
            Name = "existing PERSON",
            NewName = "New Name"
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingPerson.Id, command.Person?.Id);
    }

    [Fact]
    public async Task ProcessAndHandle_AllowFixingTheCapitalizationOfTheirOwnName()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("jane doe");
        var command = new UpdatePerson
        {
            Name = existingPerson.Name,
            NewName = "Jane Doe"
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);
        var handler = new UpdatePersonHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedPerson = await _fixture.Context.People.AsNoTracking().SingleAsync(z => z.Id == existingPerson.Id);
        Assert.Equal("Jane Doe", updatedPerson.Name);
    }

    [Fact]
    public async Task Process_DoesNotThrowException_WhenValidRequest()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new UpdatePerson
        {
            Name = existingPerson.Name,
            NewName = "New Name"
        };
        var preProcessor = new UpdatePersonPreProcessor(_fixture.Context);

        // Act & Assert
        var ex = await Record.ExceptionAsync(() => preProcessor.Process(command, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task Handle_UpdatesPersonNameSuccessfully()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new UpdatePerson
        {
            Name = existingPerson.Name,
            NewName = "Updated Name",
            Person = existingPerson
        };
        var handler = new UpdatePersonHandler(_fixture.Context);

        // Act
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingPerson.Id, result.Id);
        var updatedPerson = await _fixture.Context.People.FindAsync(existingPerson.Id);
        Assert.Equal("Updated Name", updatedPerson.Name);
    }
}
