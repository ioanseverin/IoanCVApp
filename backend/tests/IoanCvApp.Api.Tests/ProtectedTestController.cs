using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace IoanCvApp.Api.Tests;

[ApiController]
[Route("test/protected")]
[Authorize(Policy = "Admin")]
public sealed class ProtectedTestController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok(new { authorized = true });
}
