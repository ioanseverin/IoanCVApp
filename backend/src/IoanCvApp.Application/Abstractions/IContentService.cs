using IoanCvApp.Application.Dtos;

namespace IoanCvApp.Application.Abstractions;

public interface IContentService
{
    Task<ProfileDto?> GetProfileAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ExperienceEntryDto>> GetExperienceAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default);
}
