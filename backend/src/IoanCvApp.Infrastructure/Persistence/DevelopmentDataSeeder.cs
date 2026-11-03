using IoanCvApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace IoanCvApp.Infrastructure.Persistence;

public static class DevelopmentDataSeeder
{
    public static async Task SeedAsync(AppDbContext db, CancellationToken ct = default)
    {
        await db.Database.MigrateAsync(ct);

        if (!await db.Profiles.AnyAsync(ct))
        {
            db.Profiles.Add(new Profile
            {
                Name = "Sample Developer",
                Headline = "Software Engineer",
                Bio = "Sample profile used for local development.",
                Skills = ["C#", ".NET", "SQL"]
            });
        }

        if (!await db.ExperienceEntries.AnyAsync(ct))
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

        if (!await db.Projects.AnyAsync(ct))
        {
            db.Projects.Add(new Project
            {
                Name = "IoanCvApp",
                Description = "Sample project entry for local development.",
                TechStack = ["ASP.NET Core", "EF Core", "React"],
                SortOrder = 1
            });
        }

        await db.SaveChangesAsync(ct);
    }
}
