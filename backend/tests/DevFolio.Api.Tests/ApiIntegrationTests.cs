using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using DevFolio.Infrastructure.Data;
using StackExchange.Redis;

namespace DevFolio.Api.Tests;

public class ApiIntegrationTests : IClassFixture<ApiIntegrationTests.SharedFactory>
{
    private readonly SharedFactory _factory;
    private readonly HttpClient _client;

    public ApiIntegrationTests(SharedFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    // ---------- Auth (3) ----------

    [Fact]
    public async Task Login_WithValidCredentials_Returns200AndToken()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "admin123"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        var token = json.GetProperty("token").GetString();
        Assert.False(string.IsNullOrWhiteSpace(token));
    }

    [Fact]
    public async Task Login_WithWrongPassword_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "wrong"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_WithUnknownUser_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "nobody",
            password = "whatever"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    // ---------- Projects: protected endpoint (2) ----------

    [Fact]
    public async Task PostProject_WithoutToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/Projects", new
        {
            title = "Test",
            description = "No auth",
            imageUrl = "",
            techStack = "Java",
            gitHubUrl = ""
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task PostProject_WithValidToken_ReturnsCreatedAndProjectIsPersisted()
    {
        var token = await GetTokenAsync();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var title = $"Created_{Guid.NewGuid():N}";
        var post = await _client.PostAsJsonAsync("/api/Projects", new
        {
            title,
            description = "Created in test",
            imageUrl = "",
            techStack = "Java, Spring",
            gitHubUrl = ""
        });

        Assert.Equal(HttpStatusCode.Created, post.StatusCode);

        var list = await _client.GetFromJsonAsync<List<JsonElement>>("/api/Projects");
        Assert.NotNull(list);
        Assert.Contains(list!, p => p.GetProperty("title").GetString() == title);
    }

    // ---------- Projects: public read (2) ----------

    [Fact]
    public async Task GetProjects_Returns200AndArray()
    {
        var response = await _client.GetAsync("/api/Projects");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var list = await response.Content.ReadFromJsonAsync<List<JsonElement>>();
        Assert.NotNull(list);
    }

    [Fact]
    public async Task GetProjectById_WithUnknownId_Returns404()
    {
        var response = await _client.GetAsync("/api/Projects/999999");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // ---------- Contact: Redis rate limiting (2) ----------

    [Fact]
    public async Task ContactForm_FirstSubmission_Returns200()
    {
        await FlushRateLimitKeysAsync();
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.PostAsJsonAsync("/api/Contact", new
        {
            name = "Test",
            email = "test@example.com",
            message = "hello"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ContactForm_ExceedingRateLimit_Returns429()
    {
        await FlushRateLimitKeysAsync();
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        // Rate limit is 3 per IP per 15 minutes; the 4th submission should be 429.
        HttpStatusCode last = HttpStatusCode.OK;
        for (var i = 0; i < 4; i++)
        {
            var response = await client.PostAsJsonAsync("/api/Contact", new
            {
                name = "RateLimit",
                email = "rate@example.com",
                message = "spam"
            });
            last = response.StatusCode;
        }

        Assert.Equal(HttpStatusCode.TooManyRequests, last);
    }

    // ---------- Helpers ----------

    private async Task<string> GetTokenAsync()
    {
        var response = await _client.PostAsJsonAsync("/api/Auth/login", new
        {
            username = "admin",
            password = "admin123"
        });
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        return json.GetProperty("token").GetString()!;
    }

    private static async Task FlushRateLimitKeysAsync()
    {
        using var redis = ConnectionMultiplexer.Connect("localhost:6379");
        var db = redis.GetDatabase();
        var server = redis.GetServer(redis.GetEndPoints()[0]);
        foreach (var key in server.Keys(pattern: "ratelimit:*"))
        {
            await db.KeyDeleteAsync(key);
        }
    }

    // Stamps a real client IP onto every in-memory request, without replacing the pipeline.
    public class RemoteIpStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Use(async (context, nextMiddleware) =>
                {
                    if (context.Connection.RemoteIpAddress is null)
                    {
                        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
                    }
                    await nextMiddleware();
                });
                next(app);
            };
        }
    }

    public class SharedFactory : WebApplicationFactory<Program>
    {
        private readonly string _dbPath = Path.Combine(Path.GetTempPath(), $"devfolio-test-{Guid.NewGuid():N}.db");

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={_dbPath}"
                });
            });

            builder.ConfigureServices(services =>
            {
                services.AddTransient<IStartupFilter, RemoteIpStartupFilter>();
            });
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                try { File.Delete(_dbPath); } catch { }
            }
        }
    }
}