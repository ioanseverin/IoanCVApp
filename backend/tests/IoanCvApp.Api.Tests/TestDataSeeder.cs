using IoanCvApp.Domain;
using IoanCvApp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace IoanCvApp.Api.Tests;

public static class TestDataSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        if (!await db.Profiles.AnyAsync())
        {
            db.Profiles.Add(new Profile
            {
                Name = "Sample Developer",
                Headline = "Software Engineer",
                Bio = "Sample profile used for tests.",
                Skills = ["C#", ".NET", "SQL"]
            });
        }

        if (!await db.ExperienceEntries.AnyAsync())
        {
            db.ExperienceEntries.AddRange(
                new ExperienceEntry
                {
                    Title = "Senior Developer",
                    Company = "Example Corp",
                    StartDate = new DateOnly(2022, 1, 1),
                    Description = "Sample current role.",
                    TechTags = [".NET", "Azure"],
                    SortOrder = 1
                },
                new ExperienceEntry
                {
                    Title = "Developer",
                    Company = "Sample GmbH",
                    StartDate = new DateOnly(2018, 6, 1),
                    EndDate = new DateOnly(2021, 12, 31),
                    Description = "Sample previous role.",
                    TechTags = ["C#", "SQL"],
                    SortOrder = 2
                });
        }

        if (!await db.Projects.AnyAsync())
        {
            db.Projects.Add(new Project
            {
                Name = "IoanCvApp",
                Description = "Sample project entry for tests.",
                TechStack = ["ASP.NET Core", "EF Core", "React"],
                SortOrder = 1
            });
        }

        await db.SaveChangesAsync();
    }
}
