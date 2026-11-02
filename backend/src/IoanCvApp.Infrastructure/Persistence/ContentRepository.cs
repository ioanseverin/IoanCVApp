using IoanCvApp.Application.Abstractions;
using IoanCvApp.Domain;
using Microsoft.EntityFrameworkCore;

namespace IoanCvApp.Infrastructure.Persistence;

public class ContentRepository(AppDbContext db) : IContentRepository
{
    public Task<Profile?> GetProfileAsync(CancellationToken ct = default) =>
        db.Profiles.AsNoTracking().FirstOrDefaultAsync(ct);

    public async Task<IReadOnlyList<ExperienceEntry>> GetExperienceAsync(CancellationToken ct = default) =>
        await db.ExperienceEntries.AsNoTracking()
            .OrderBy(e => e.SortOrder).ThenByDescending(e => e.StartDate)
            .ToListAsync(ct);

    public async Task<IReadOnlyList<Project>> GetProjectsAsync(CancellationToken ct = default) =>
        await db.Projects.AsNoTracking().OrderBy(p => p.SortOrder).ThenBy(p => p.Id).ToListAsync(ct);
}
