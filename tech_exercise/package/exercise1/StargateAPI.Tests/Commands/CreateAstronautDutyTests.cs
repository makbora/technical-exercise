namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;
using StargateAPI.Business.Commands;
using StargateAPI.Business.Data;

public class CreateAstronautDutyTests : IClassFixture<TestDbContextFixture>
{
    private readonly TestDbContextFixture _fixture;

    public CreateAstronautDutyTests(TestDbContextFixture fixture)
    {
        _fixture = new TestDbContextFixture();
    }

    [Fact]
    public async Task Process_ThrowsException_WhenPersonDoesNotExist()
    {
        // Arrange
        var command = new CreateAstronautDuty
        {
            Name = "Non-existent Person",
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_ThrowsException_WhenPersonAlreadyHasDutyScheduled()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        
        var existingDuty = _fixture.CreateTestAstronautDuty(existingPerson, "Captain", "Mission Specialist",  new DateTime(2026, 10, 28, 14, 30, 0));

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = new DateTime(2026, 10, 28, 14, 30, 0)
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);

        // Act & Assert
        await Assert.ThrowsAsync<BadHttpRequestException>(() => preProcessor.Process(command, CancellationToken.None));
    }

    [Fact]
    public async Task Process_DoesNotThrowException_WhenValidRequest()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);

        // Act & Assert
        var ex = await Record.ExceptionAsync(() => preProcessor.Process(command, CancellationToken.None));
        Assert.Null(ex);
    }

    [Fact]
    public async Task Handle_CreatesAstronautDutySuccessfully()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        Assert.Equal(existingPerson.Id, result.Id);
        var updatedAstronautDetail = await _fixture.Context.AstronautDetails.FirstOrDefaultAsync(z => z.PersonId == existingPerson.Id);
        Assert.NotNull(updatedAstronautDetail);
        Assert.Equal("Mission Specialist", updatedAstronautDetail.CurrentDutyTitle);
        Assert.Equal("Captain", updatedAstronautDetail.CurrentRank);
    }

    [Fact]
    public async Task Handle_UpdatesAstronautDetailSuccessfully_WhenAstronautDetailExists()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var existingAstronautDetail = _fixture.CreateTestAstronautDetail(existingPerson, "Lieutenant", "Old Duty", DateTime.UtcNow.AddYears(-5));

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedAstronautDetail = await _fixture.Context.AstronautDetails.FirstOrDefaultAsync(z => z.PersonId == existingPerson.Id);
        Assert.NotNull(updatedAstronautDetail);
        Assert.Equal("Mission Specialist", updatedAstronautDetail.CurrentDutyTitle);
        Assert.Equal("Captain", updatedAstronautDetail.CurrentRank);
    }

    [Fact]
    public async Task Handle_UpdatesPreviousAstronautDutySuccessfully_WhenPreviousDutyExists()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Existing Person");
        var existingAstronautDuty = new AstronautDuty
        {
            PersonId = existingPerson.Id,
            Rank = "Lieutenant",
            DutyTitle = "Old Duty",
            DutyStartDate = DateTime.UtcNow.AddDays(-10),
            DutyEndDate = null
        };
        _fixture.Context.AstronautDuties.Add(existingAstronautDuty);
        await _fixture.Context.SaveChangesAsync();

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "Captain",
            DutyTitle = "Mission Specialist",
            DutyStartDate = DateTime.UtcNow
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var updatedPreviousDuty = await _fixture.Context.AstronautDuties.FirstOrDefaultAsync(z => z.PersonId == existingPerson.Id && z.DutyEndDate != null);
        Assert.NotNull(updatedPreviousDuty);
        Assert.Equal(command.DutyStartDate.AddDays(-1).Date, updatedPreviousDuty.DutyEndDate.Value.Date);
    }
}