using Microsoft.AspNetCore.Mvc;
using DevFolio.Infrastructure.Services;

namespace DevFolio.Api.Controllers;

public class ContactRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}

[ApiController]
[Route("api/[controller]")]
public class ContactController : ControllerBase
{
    private readonly RedisService _redis;

    public ContactController(RedisService redis)
    {
        _redis = redis;
    }

    [HttpPost]
    public async Task<IActionResult> SubmitContactForm([FromBody] ContactRequest request)
    {
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (await _redis.IsRateLimitedAsync(ipAddress))
        {
            return StatusCode(429, "Too many requests. Please try again later.");
        }

        // TODO: persist to SQLite / send email
        return Ok(new { Message = "Contact form submitted successfully!", YourIp = ipAddress });
    }
}