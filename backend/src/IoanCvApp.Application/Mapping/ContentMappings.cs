using IoanCvApp.Application.Dtos;
using IoanCvApp.Domain;

namespace IoanCvApp.Application.Mapping;

public static class ContentMappings
{
    public static ProfileDto ToDto(this Profile p) =>
        new(p.Name, p.Headline, p.Bio, p.Skills.ToArray(), p.LinkedInUrl, p.GitHubUrl, p.Email, p.ResumeUrl);

    public static ExperienceEntryDto ToDto(this ExperienceEntry e) =>
        new(e.Id, e.Title, e.Company, e.StartDate, e.EndDate, e.Description, e.TechTags.ToArray());

    public static ProjectDto ToDto(this Project p) =>
        new(p.Id, p.Name, p.Description, p.TechStack.ToArray(), p.RepositoryUrl, p.LiveUrl, p.ImageUrl);
}
