using Microsoft.AspNetCore.Mvc;
using DevFolio.Infrastructure.Services;

namespace DevFolio.Api.Controllers;

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
    public async Task<IActionResult> SubmitContactForm([FromBody] string message)
    {
        // Get the user's IP address
        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        // Check Redis to see if they are spamming
        if (await _redis.IsRateLimitedAsync(ipAddress))
        {
            return StatusCode(429, "Too many requests. Please try again later.");
        }

        // In a real app, you would save this to SQLite or send an email here.
        // For now, we just prove the rate limiter works.
        return Ok(new { Message = "Contact form submitted successfully!", YourIp = ipAddress });
    }
}