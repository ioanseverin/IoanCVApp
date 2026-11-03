using IoanCvApp.Application.Abstractions;
using IoanCvApp.Application.Dtos;
using IoanCvApp.Application.Mapping;

namespace IoanCvApp.Application.Services;

public sealed class ContentService(IContentRepository repository) : IContentService
{
    public async Task<ProfileDto?> GetProfileAsync(CancellationToken ct = default) =>
        (await repository.GetProfileAsync(ct))?.ToDto();

    public async Task<IReadOnlyList<ExperienceEntryDto>> GetExperienceAsync(CancellationToken ct = default) =>
        (await repository.GetExperienceAsync(ct)).Select(e => e.ToDto()).ToArray();

    public async Task<IReadOnlyList<ProjectDto>> GetProjectsAsync(CancellationToken ct = default) =>
        (await repository.GetProjectsAsync(ct)).Select(p => p.ToDto()).ToArray();
}
