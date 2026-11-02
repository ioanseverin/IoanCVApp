namespace IoanCvApp.Application.Dtos;

public record ProfileDto(
    string Name,
    string Headline,
    string Bio,
    IReadOnlyList<string> Skills,
    string? LinkedInUrl,
    string? GitHubUrl,
    string? Email,
    string? ResumeUrl);

public record ExperienceEntryDto(
    int Id,
    string Title,
    string Company,
    DateOnly StartDate,
    DateOnly? EndDate,
    string Description,
    IReadOnlyList<string> TechTags);

public record ProjectDto(
    int Id,
    string Name,
    string Description,
    IReadOnlyList<string> TechStack,
    string? RepositoryUrl,
    string? LiveUrl,
    string? ImageUrl);
