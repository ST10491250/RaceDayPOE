
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDayApi.Data;
using System.Security.Claims;

namespace RaceDayApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Organiser")]
    public class OrganiserController : ControllerBase
    {
        private readonly RaceDayDbContext _context;

        public OrganiserController(RaceDayDbContext context)
        {
            _context = context;
        }

        // Get the dashboard information for the logged-in organiser
        [HttpGet("dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            int userId = int.Parse(
                User.FindFirst(ClaimTypes.NameIdentifier)!.Value);

            var organiser = await _context.Organisers
                .FirstOrDefaultAsync(o => o.UserId == userId);

            if (organiser == null)
                return NotFound(new { message = "Organiser not found." });

            var events = await _context.Events
                .Where(e => e.OrganiserId == organiser.OrganiserId)
                .Select(e => new
                {
                    e.EventId,
                    e.Name,
                    e.Description,
                    e.EventDate,
                    TotalEnrolments = e.Enrolments.Count()
                })
                .OrderBy(e => e.EventDate)
                .ToListAsync();

            return Ok(new
            {
                totalEvents = events.Count,
                totalEnrolments = events.Sum(e => e.TotalEnrolments),
                upcomingEvents = events
                    .Where(e => e.EventDate >= DateTime.Now)
                    .ToList(),
                allEvents = events
            });
        }
    }
}
