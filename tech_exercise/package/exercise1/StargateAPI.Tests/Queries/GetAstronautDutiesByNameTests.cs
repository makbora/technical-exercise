namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Queries;

public class GetAstronautDutiesByNameTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;

    public GetAstronautDutiesByNameTests(TestDbContextFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task Processor_ThrowsException_WhenPersonDoesNotExist()
    {
        // Arrange
        var query = new GetAstronautDutiesByName
        {
            Name = "Non-existent Person"
        };
        var processor = new GetAstronautDutiesByNamePreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => processor.Process(query, CancellationToken.None));
    }

    [Fact]
    public async Task ReturnsResult_WhenPersonExists()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var query = new GetAstronautDutiesByName
        {
            Name = existingPerson.Name
        };
        var processor = new GetAstronautDutiesByNamePreProcessor(_fixture.Context);
        var handler = new GetAstronautDutiesByNameHandler(_fixture.Context);

        // Act
        await processor.Process(query, CancellationToken.None);
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Person);
        Assert.Equal(existingPerson.Name, result.Person.Name);
    }

    [Fact]
    public async Task Processor_ThrowsException_WhenNameIsEmpty()
    {
        // Arrange
        var query = new GetAstronautDutiesByName
        {
            Name = ""
        };
        var processor = new GetAstronautDutiesByNamePreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => processor.Process(query, CancellationToken.None));
    }

    [Fact]
    public async Task Processor_ThrowsException_WhenNameIsWhitespace()
    {
        // Arrange
        var query = new GetAstronautDutiesByName
        {
            Name = "   "
        };
        var processor = new GetAstronautDutiesByNamePreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => processor.Process(query, CancellationToken.None));
    }
}