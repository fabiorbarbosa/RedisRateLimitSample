using Microsoft.AspNetCore.Mvc;
using RateLimit.Redis.Sample.Attributes;

namespace RateLimit.Redis.Sample.Controllers;

[ApiController]
[Route("auth")]
public sealed class AuthController : ControllerBase
{
    [HttpPost("issue-token")]
    [EnsureRateLimitToken("FixedApi")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public IActionResult IssueToken() => Ok(new { ok = true });
}
