using Microsoft.AspNetCore.Mvc;
using RateLimit.Redis.Sample.Attributes;

namespace RateLimit.Redis.Sample.Controllers;

[ApiController]
[Route("api")]
public sealed class SampleController : ControllerBase
{
    [HttpGet("dados")]
    [FixedWindowRateLimit("FixedApi")]
    public IActionResult Dados()
        => Ok(new { now = DateTimeOffset.UtcNow });
}
