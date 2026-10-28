namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Queries;

public class GetPersonByNameTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;

    public GetPersonByNameTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ThrowsException_WhenPersonDoesNotExist()
    {
        // Arrange
        var query = new GetPersonByName
        {
            Name = "Non-existent Person"
        };
        var handler = new GetPersonByNameHandler(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task ReturnsResult_WhenPersonExists()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var query = new GetPersonByName
        {
            Name = existingPerson.Name
        };
        var handler = new GetPersonByNameHandler(_fixture.Context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Person);
        Assert.Equal(existingPerson.Name, result.Person.Name);
    }

    [Fact]
    public async Task ThrowsException_WhenNameIsEmpty()
    {
        // Arrange
        var query = new GetPersonByName
        {
            Name = ""
        };
        var handler = new GetPersonByNameHandler(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(query, CancellationToken.None));
    }

    [Fact]
    public async Task ThrowsException_WhenNameIsWhitespace()
    {
        // Arrange
        var query = new GetPersonByName
        {
            Name = "   "
        };
        var handler = new GetPersonByNameHandler(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => handler.Handle(query, CancellationToken.None));
    }
}