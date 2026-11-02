using IoanCvApp.Application.Mapping;
using IoanCvApp.Domain;
using IoanCvApp.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace IoanCvApp.Api.Tests;

public sealed class PersistenceTests : IDisposable
{
    private readonly SqliteConnection _connection = new("Data Source=:memory:");
    private readonly DbContextOptions<AppDbContext> _options;

    public PersistenceTests()
    {
        _connection.Open();
        _options = new DbContextOptionsBuilder<AppDbContext>().UseSqlite(_connection).Options;
        using var db = new AppDbContext(_options);
        db.Database.Migrate();
    }

    [Fact]
    public async Task Migration_allows_storing_and_reading_all_content_entities()
    {
        await using (var db = new AppDbContext(_options))
        {
            db.Profiles.Add(new Profile
            {
                Name = "Ioan",
                Headline = "Developer",
                Bio = "Bio",
                Skills = ["C#", "React"]
            });
            db.ExperienceEntries.Add(new ExperienceEntry
            {
                Title = "Dev",
                Company = "Acme",
                StartDate = new DateOnly(2020, 1, 1),
                Description = "Work",
                TechTags = [".NET", "SQL"]
            });
            db.Projects.Add(new Project
            {
                Name = "CV",
                Description = "Site",
                TechStack = ["ASP.NET Core", "SQLite"]
            });
            await db.SaveChangesAsync();
        }

        await using var read = new AppDbContext(_options);
        var repository = new ContentRepository(read);

        var profile = await repository.GetProfileAsync();
        var experience = await repository.GetExperienceAsync();
        var projects = await repository.GetProjectsAsync();

        Assert.Equal(["C#", "React"], profile!.Skills);
        Assert.Equal(new DateOnly(2020, 1, 1), Assert.Single(experience).StartDate);
        Assert.Equal([".NET", "SQL"], experience[0].TechTags);
        Assert.Equal(["ASP.NET Core", "SQLite"], Assert.Single(projects).TechStack);
    }

    [Fact]
    public void Dto_mapping_exposes_only_explicit_fields()
    {
        var project = new Project { Id = 7, Name = "CV", Description = "Site", TechStack = ["C#"], SortOrder = 3 };

        var dto = project.ToDto();

        Assert.Equal(7, dto.Id);
        Assert.Equal(["C#"], dto.TechStack);
        Assert.DoesNotContain(typeof(Project).GetProperty(nameof(Project.SortOrder))!.Name,
            dto.GetType().GetProperties().Select(p => p.Name));
    }

    public void Dispose() => _connection.Dispose();
}
