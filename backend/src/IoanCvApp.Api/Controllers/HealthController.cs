using IoanCvApp.Api.Contracts;
using Microsoft.AspNetCore.Mvc;

namespace IoanCvApp.Api.Controllers;

[ApiController]
[Route("health")]
public sealed class HealthController : ControllerBase
{
    /// <summary>Check API liveness.</summary>
    /// <remarks>Returns 200 OK when the API process is running and able to serve requests.</remarks>
    [HttpGet(Name = "GetHealth")]
    [ProducesResponseType<HealthResponse>(StatusCodes.Status200OK)]
    public ActionResult<HealthResponse> Get() => Ok(new HealthResponse("Healthy"));
}
