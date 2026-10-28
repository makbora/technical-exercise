namespace StargateAPI.Tests;

using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using StargateAPI.Business.Data;

public sealed class TestDbContextFixture : IDisposable
{
    private readonly SqliteConnection _connection;
    public StargateContext Context { get; private set; }

    public TestDbContextFixture()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<StargateContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new StargateContext(options);
        Context.Database.EnsureCreated();
    }

    public Person CreateTestPerson(string name)
    {
        var person = new Person
        {
            Name = name
        };

        Context.People.Add(person);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        return person;
    }

    public AstronautDetail CreateTestAstronautDetail(Person person, string currentRank, string currentDutyTitle, DateTime careerStartDate, DateTime? careerEndDate = null)
    {
        var astronautDetail = new AstronautDetail
        {
            PersonId = person.Id,
            CurrentRank = currentRank,
            CurrentDutyTitle = currentDutyTitle,
            CareerStartDate = careerStartDate,
            CareerEndDate = careerEndDate
        };

        Context.AstronautDetails.Add(astronautDetail);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        return astronautDetail;
    }

    public AstronautDuty CreateTestAstronautDuty(Person person, string rank, string dutyTitle, DateTime dutyStartDate, DateTime? dutyEndDate = null)
    {
        var astronautDuty = new AstronautDuty
        {
            PersonId = person.Id,
            Rank = rank,
            DutyTitle = dutyTitle,
            DutyStartDate = dutyStartDate,
            DutyEndDate = dutyEndDate
        };

        Context.AstronautDuties.Add(astronautDuty);
        Context.SaveChanges();
        Context.ChangeTracker.Clear();

        return astronautDuty;
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Close();
    }
}
