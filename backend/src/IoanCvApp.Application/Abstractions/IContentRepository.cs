using IoanCvApp.Domain;

namespace IoanCvApp.Application.Abstractions;

public interface IContentRepository
{
    Task<Profile?> GetProfileAsync(CancellationToken ct = default);
    Task<IReadOnlyList<ExperienceEntry>> GetExperienceAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken ct = default);
}
