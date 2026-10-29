using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using DevFolio.Core.Entities;
using DevFolio.Infrastructure.Data;
using DevFolio.Infrastructure.Services;

namespace DevFolio.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProjectsController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly RedisService _redis;

    public ProjectsController(AppDbContext context, RedisService redis)
    {
        _context = context;
        _redis = redis;
    }

    // GET: api/projects
    [HttpGet]
    public async Task<ActionResult<IEnumerable<Project>>> GetProjects()
    {
        return await _context.Projects.ToListAsync();
    }

    // GET: api/projects/5
    [HttpGet("{id}")]
    public async Task<ActionResult<Project>> GetProject(int id)
    {
        var project = await _context.Projects.FindAsync(id);

        if (project == null) return NotFound();

        // NO SQL MAGIC: Increment the view count in Redis
        await _redis.IncrementProjectViewAsync(id);

        return project;
    }

    // GET: api/projects/5/views
    [HttpGet("{id}/views")]
    public async Task<ActionResult<long>> GetProjectViews(int id)
    {
        var views = await _redis.GetProjectViewsAsync(id);
        return Ok(new { ProjectId = id, Views = views });
    }

    // POST: api/projects
    [Authorize]
    [HttpPost]
    public async Task<ActionResult<Project>> CreateProject(Project project)
    {
        _context.Projects.Add(project);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }
}