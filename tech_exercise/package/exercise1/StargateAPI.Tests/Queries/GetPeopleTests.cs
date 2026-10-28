namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Queries;

public class GetPeopleTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;

    public GetPeopleTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task ReturnsResult_WhenPeopleExist()
    {
        // Arrange
        var existingPerson1 = _fixture.CreateTestPerson("Existing Person 1");
        var existingPerson2 = _fixture.CreateTestPerson("Existing Person 2");
        var query = new GetPeople();
        var handler = new GetPeopleHandler(_fixture.Context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.People);
        Assert.Contains(result.People, p => p.Name == existingPerson1.Name);
        Assert.Contains(result.People, p => p.Name == existingPerson2.Name);
    }

    [Fact]
    public async Task ReturnsEmptyResult_WhenNoPeopleExist()
    {
        // Arrange
        var query = new GetPeople();
        var handler = new GetPeopleHandler(_fixture.Context);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.People);
        Assert.Empty(result.People);
    }
}