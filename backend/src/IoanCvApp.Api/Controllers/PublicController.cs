using IoanCvApp.Application.Abstractions;
using IoanCvApp.Application.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace IoanCvApp.Api.Controllers;

[ApiController]
[Route("api/public")]
[Tags("Public")]
public sealed class PublicController(IContentService content) : ControllerBase
{
    /// <summary>Get the About/CV profile.</summary>
    /// <response code="404">No profile content exists yet.</response>
    [HttpGet("profile", Name = "GetPublicProfile")]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProfileDto>> GetProfile(CancellationToken ct)
    {
        var profile = await content.GetProfileAsync(ct);
        return profile is null
            ? NotFound(new ProblemDetails
            {
                Status = StatusCodes.Status404NotFound,
                Title = "Profile not found",
                Detail = "No profile content has been published yet."
            })
            : Ok(profile);
    }

    /// <summary>Get experience entries in display order.</summary>
    /// <remarks>Returns an empty array when no entries exist.</remarks>
    [HttpGet("experience", Name = "GetPublicExperience")]
    [ProducesResponseType<IReadOnlyList<ExperienceEntryDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ExperienceEntryDto>>> GetExperience(CancellationToken ct) =>
        Ok(await content.GetExperienceAsync(ct));

    /// <summary>Get project entries.</summary>
    /// <remarks>Returns an empty array when no projects exist.</remarks>
    [HttpGet("projects", Name = "GetPublicProjects")]
    [ProducesResponseType<IReadOnlyList<ProjectDto>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProjectDto>>> GetProjects(CancellationToken ct) =>
        Ok(await content.GetProjectsAsync(ct));
}
