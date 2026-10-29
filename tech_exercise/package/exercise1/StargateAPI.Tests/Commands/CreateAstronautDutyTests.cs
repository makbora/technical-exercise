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
        // Another person's duty, so the new duty's Id can't coincide with the person's Id.
        var otherPerson = _fixture.CreateTestPerson("Other Person");
        _fixture.CreateTestAstronautDuty(otherPerson, "Major", "Commander", DateTime.UtcNow.AddYears(-1));

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
        var createdDuty = await _fixture.Context.AstronautDuties.AsNoTracking().SingleAsync(z => z.PersonId == existingPerson.Id);
        Assert.Equal(createdDuty.Id, result.Id);
        Assert.Equal("Captain", createdDuty.Rank);
        Assert.Equal("Mission Specialist", createdDuty.DutyTitle);
        Assert.Null(createdDuty.DutyEndDate);
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

    [Fact]
    public async Task Handle_SetsCareerEndDateToDayBeforeRetirement_WhenAstronautRetires()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Retiring Astronaut");
        _fixture.CreateTestAstronautDetail(existingPerson, "COL", "Commander", new DateTime(2015, 3, 1));
        _fixture.CreateTestAstronautDuty(existingPerson, "COL", "Commander", new DateTime(2015, 3, 1));

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "COL",
            DutyTitle = "RETIRED",
            DutyStartDate = new DateTime(2025, 6, 1, 9, 30, 0)
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var astronautDetail = await _fixture.Context.AstronautDetails.AsNoTracking().SingleAsync(z => z.PersonId == existingPerson.Id);
        Assert.Equal("RETIRED", astronautDetail.CurrentDutyTitle);
        Assert.Equal(new DateTime(2015, 3, 1), astronautDetail.CareerStartDate);
        Assert.Equal(new DateTime(2025, 5, 31), astronautDetail.CareerEndDate);
    }

    [Fact]
    public async Task Handle_SetsCareerEndDateToDayBeforeRetirement_WhenFirstDutyIsRetired()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Retired On Arrival");

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "MAJ",
            DutyTitle = "RETIRED",
            DutyStartDate = new DateTime(2025, 6, 1)
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var astronautDetail = await _fixture.Context.AstronautDetails.AsNoTracking().SingleAsync(z => z.PersonId == existingPerson.Id);
        Assert.Equal("RETIRED", astronautDetail.CurrentDutyTitle);
        Assert.Equal(new DateTime(2025, 6, 1), astronautDetail.CareerStartDate);
        Assert.Equal(new DateTime(2025, 5, 31), astronautDetail.CareerEndDate);
    }

    [Fact]
    public async Task Handle_ClearsCareerEndDate_WhenRetiredAstronautReturnsToDuty()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Returning Astronaut");
        _fixture.CreateTestAstronautDetail(existingPerson, "COL", "RETIRED", new DateTime(2010, 1, 1), new DateTime(2020, 11, 30));
        _fixture.CreateTestAstronautDuty(existingPerson, "COL", "Commander", new DateTime(2010, 1, 1), new DateTime(2020, 11, 30));
        _fixture.CreateTestAstronautDuty(existingPerson, "COL", "RETIRED", new DateTime(2020, 12, 1));

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "COL",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2025, 1, 1)
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        var result = await handler.Handle(command, CancellationToken.None);

        // Assert
        var astronautDetail = await _fixture.Context.AstronautDetails.AsNoTracking().SingleAsync(z => z.PersonId == existingPerson.Id);
        Assert.Equal("Pilot", astronautDetail.CurrentDutyTitle);
        Assert.Equal(new DateTime(2010, 1, 1), astronautDetail.CareerStartDate);
        Assert.Null(astronautDetail.CareerEndDate);

        // The retirement stays in the history, closed the day before the return.
        var retirement = await _fixture.Context.AstronautDuties.AsNoTracking().SingleAsync(z => z.PersonId == existingPerson.Id && z.DutyTitle == "RETIRED");
        Assert.Equal(new DateTime(2024, 12, 31), retirement.DutyEndDate);
        var newDuty = await _fixture.Context.AstronautDuties.AsNoTracking().SingleAsync(z => z.Id == result.Id);
        Assert.Null(newDuty.DutyEndDate);
    }

    [Fact]
    public async Task Handle_LeavesCareerEndDateEmpty_WhenDutyIsNotRetirement()
    {
        // Arrange
        var existingPerson = _fixture.CreateTestPerson("Active Astronaut");

        var command = new CreateAstronautDuty
        {
            Name = existingPerson.Name,
            Rank = "CPT",
            DutyTitle = "Pilot",
            DutyStartDate = new DateTime(2025, 6, 1)
        };
        var preProcessor = new CreateAstronautDutyPreProcessor(_fixture.Context);
        var handler = new CreateAstronautDutyHandler(_fixture.Context);

        // Act
        await preProcessor.Process(command, CancellationToken.None);
        await handler.Handle(command, CancellationToken.None);

        // Assert
        var astronautDetail = await _fixture.Context.AstronautDetails.AsNoTracking().SingleAsync(z => z.PersonId == existingPerson.Id);
        Assert.Null(astronautDetail.CareerEndDate);
    }
}