using System.Net;
using System.Net.Http.Json;
using IoanCvApp.Application.Dtos;
using IoanCvApp.Domain;

namespace IoanCvApp.Api.Tests;

public sealed class PublicEndpointTests
{
    [Fact]
    public async Task Empty_database_returns_empty_lists_and_problem_details_for_profile()
    {
        using var factory = new TestApiFactory();
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var experience = await client.GetFromJsonAsync<List<ExperienceEntryDto>>("/api/public/experience");
        var projects = await client.GetFromJsonAsync<List<ProjectDto>>("/api/public/projects");
        var profile = await client.GetAsync("/api/public/profile");

        Assert.Empty(experience!);
        Assert.Empty(projects!);
        Assert.Equal(HttpStatusCode.NotFound, profile.StatusCode);
        Assert.Equal("application/problem+json", profile.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task Seeded_content_is_returned_anonymously_in_display_order()
    {
        using var factory = new TestApiFactory();
        await factory.InitializeDatabaseAsync(async db =>
        {
            db.Profiles.Add(new Profile
            {
                Name = "Ioan",
                Headline = "Developer",
                Bio = "Bio",
                Skills = ["C#", "React"],
                GitHubUrl = "https://github.com/example"
            });
            db.ExperienceEntries.AddRange(
                new ExperienceEntry { Title = "Second", Company = "B", StartDate = new DateOnly(2019, 1, 1), SortOrder = 2, TechTags = ["SQL"] },
                new ExperienceEntry { Title = "First", Company = "A", StartDate = new DateOnly(2022, 3, 1), SortOrder = 1, TechTags = [".NET"] });
            db.Projects.AddRange(
                new Project { Name = "P2", Description = "d", TechStack = ["React"], ImageUrl = "https://example.com/p2.png", SortOrder = 2 },
                new Project { Name = "P1", Description = "d", TechStack = ["ASP.NET Core", "EF Core"], SortOrder = 1 });
            await db.SaveChangesAsync();
        });
        using var client = factory.CreateClient();

        var profile = await client.GetFromJsonAsync<ProfileDto>("/api/public/profile");
        var experience = await client.GetFromJsonAsync<List<ExperienceEntryDto>>("/api/public/experience");
        var projects = await client.GetFromJsonAsync<List<ProjectDto>>("/api/public/projects");

        Assert.Equal("Ioan", profile!.Name);
        Assert.Equal(["C#", "React"], profile.Skills);
        Assert.Equal(["First", "Second"], experience!.Select(e => e.Title));
        Assert.Equal(new DateOnly(2022, 3, 1), experience![0].StartDate);
        Assert.Equal(["P1", "P2"], projects!.Select(p => p.Name));
        Assert.Equal(["ASP.NET Core", "EF Core"], projects![0].TechStack);
        Assert.Null(projects[0].ImageUrl);
        Assert.Equal("https://example.com/p2.png", projects[1].ImageUrl);
    }

    [Fact]
    public async Task Development_serves_seeded_content_and_documents_public_endpoints()
    {
        using var factory = new TestApiFactory("Development");
        await factory.InitializeDatabaseAsync();
        using var client = factory.CreateClient();

        var profile = await client.GetFromJsonAsync<ProfileDto>("/api/public/profile");
        var openApi = await client.GetStringAsync("/openapi/v1.json");

        Assert.Equal("Sample Developer", profile!.Name);
        Assert.Contains("/api/public/profile", openApi);
        Assert.Contains("/api/public/experience", openApi);
        Assert.Contains("/api/public/projects", openApi);
    }
}
