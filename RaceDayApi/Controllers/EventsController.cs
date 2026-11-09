using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using RaceDayApi.DTOs;
using RaceDayApi.Models;
using Microsoft.AspNetCore.Hosting;
using System.Security.Claims;

namespace RaceDayApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventsController : ControllerBase
    {
        private readonly RaceDayDbContext _context;
        private readonly IWebHostEnvironment _environment;
        public EventsController(RaceDayDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        private int GetUserId() => int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value!);

        // GET: api/events (Everyone)
        [HttpGet]
        public async Task<IActionResult> GetAllEvents()
        {
            var events = await _context.Events
                .Include(e => e.Categories)
                .Select(e => new {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location, e.Distance, e.EventType, e.BannerImage,
                    OrganiserName = e.Organiser.CompanyName,
                    Categories = e.Categories.Select(c => new { c.CategoryId, c.Name, c.CategoryType })
                })
                .ToListAsync();

            return Ok(events);
        }

        // GET: api/events/{id} (Everyone)
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEventById(int id)
        {
            var ev = await _context.Events
                .Include(e => e.Categories)
                .Include(e => e.Organiser)
                .FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });

            return Ok(new { ev.EventId,ev.Name,ev.Description,ev.EventDate,ev.Location,ev.Distance,ev.EventType,ev.BannerImage, OrganiserName=ev.Organiser.CompanyName, Categories=ev.Categories.Select(c=>new{c.CategoryId,c.Name,c.CategoryType}) });
        }

        [HttpPost]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> CreateEvent([FromBody] CreateEventDto dto)
        {
            var userId = GetUserId();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            var newEvent = new Event
            {
                OrganiserId = organiser.OrganiserId,
                Name = dto.Name.Trim(),
                Description = dto.Description?.Trim(),
                EventDate = dto.EventDate,
                Location = dto.Location,
                Distance = dto.Distance,
                EventType = dto.EventType
            };

            _context.Events.Add(newEvent);
            await _context.SaveChangesAsync();

            return StatusCode(201, new
            {
                message = "Event created.",
                eventId = newEvent.EventId
            });
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UpdateEvent(int id, [FromBody] UpdateEventDto dto)
        {
            var userId = GetUserId();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound();
            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return StatusCode(403);

            ev.Name = dto.Name.Trim();
            ev.Description = dto.Description?.Trim();
            ev.EventDate = dto.EventDate;
            ev.Location = dto.Location;
            ev.Distance = dto.Distance;
            ev.EventType = dto.EventType;

            await _context.SaveChangesAsync();

            return Ok(new { message = "Event updated." });
        }

        // DELETE: api/events/{id} (Organiser Only)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> DeleteEvent(int id)
        {
            var userId = GetUserId();
            var organiser = await _context.Organisers.FirstOrDefaultAsync(o => o.UserId == userId);
            var ev = await _context.Events.FirstOrDefaultAsync(e => e.EventId == id);

            if (ev == null) return NotFound(new { message = "Event not found." });
            if (organiser == null || ev.OrganiserId != organiser.OrganiserId) return Forbidden();

            if (await _context.Enrolments.AnyAsync(e => e.EventId == id))
                return Conflict(new { message = "This event has enrolments and cannot be deleted." });
            _context.Events.Remove(ev);
            await _context.SaveChangesAsync();
            return NoContent(); 
        }


        [HttpPost("upload-banner/{id}")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> UploadBanner(int id, IFormFile image)
        {
            var userId = GetUserId();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            var ev = await _context.Events.FindAsync(id);

            if (ev == null)
                return NotFound(new { message = "Event not found." });

            if (organiser == null || ev.OrganiserId != organiser.OrganiserId)
                return Forbid();

            if (image == null || image.Length == 0)
                return BadRequest(new { message = "Please select an image." });

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Invalid image format." });

            if (image.Length > 5 * 1024 * 1024)
                return BadRequest(new { message = "Image must be smaller than 5MB." });

            var folder = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath,"wwwroot"), "event-banners");

            Directory.CreateDirectory(folder);

            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(folder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await image.CopyToAsync(stream);
            }

            ev.BannerImage = $"/event-banners/{fileName}";

            await _context.SaveChangesAsync();

            return Ok(new
            {
                message = "Banner uploaded successfully.",
                imagePath = ev.BannerImage
            });
        }

        // GET: api/events/my-events
        [HttpGet("my-events")]
        [Authorize(Roles = "Organiser")]
        public async Task<IActionResult> GetMyEvents()
        {
            var userId = GetUserId();

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return Unauthorized();

            var events = await _context.Events
                .Where(e => e.OrganiserId == organiser.OrganiserId)
                .Include(e => e.Categories)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    e.Location, e.Distance, e.EventType, e.BannerImage,
                    TotalEnrolments = e.Enrolments.Count(),
                    Categories = e.Categories.Select(c => new { c.CategoryId, c.Name, c.CategoryType })
                })
                .ToListAsync();

            return Ok(events);
        }


        private IActionResult Forbidden() => StatusCode(403, new { message = "You do not have permission to modify this event." });
    }
}