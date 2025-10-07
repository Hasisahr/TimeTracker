using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TimeTracker.DAL;
using TimeTracker.Model;

namespace TimeTracker.Web.Controllers
{
    public class ScheduleAPIController : Controller
    {
        private readonly TimeTrackerManagerDbContext _context;

        public ScheduleAPIController(TimeTrackerManagerDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [Route("api/schedules")]
        public async Task<IActionResult> GetSchedulesDto()
        {
            var schedules = await _context.Schedules
                .OrderByDescending(s => s.ScheduleDate)
                .ToListAsync();

            var dtoList = schedules.Select(s => new ScheduleDto
            {
                ScheduleDate = s.ScheduleDate,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                TicketsSold = s.TicketsSold
            }).ToList();

            return Ok(dtoList);
        }

        [HttpGet]
        [Route("api/schedules/{id}")]
        public async Task<IActionResult> GetScheduleDto(int id)
        {
            var s = await _context.Schedules.FindAsync(id);
            if (s == null)
                return NotFound();

            var dto = new ScheduleDto
            {
                ScheduleDate = s.ScheduleDate,
                StartTime = s.StartTime.ToString(@"hh\:mm"),
                EndTime = s.EndTime.ToString(@"hh\:mm"),
                TicketsSold = s.TicketsSold
            };

            return Ok(dto);
        }


        [HttpPost]
        [Route("api/schedules")]
        public async Task<IActionResult> CreateSchedule([FromBody] ScheduleDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var schedule = new Schedule
            {
                ScheduleDate = dto.ScheduleDate,
                StartTime = TimeSpan.Parse(dto.StartTime),
                EndTime = TimeSpan.Parse(dto.EndTime),
                TicketsSold = dto.TicketsSold
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetScheduleDto), new { id = schedule.Id }, dto);
        }

        [HttpPut]
        [Route("api/schedules/{id}")]
        public async Task<IActionResult> UpdateSchedule(int id, [FromBody] ScheduleDto dto)
        {
            var schedule = await _context.Schedules.FindAsync(id);
            if (schedule == null)
                return NotFound();

            schedule.ScheduleDate = dto.ScheduleDate;
            schedule.StartTime = TimeSpan.Parse(dto.StartTime);
            schedule.EndTime = TimeSpan.Parse(dto.EndTime);
            schedule.TicketsSold = dto.TicketsSold;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete]
        [Route("api/schedules/{id}")]
        public async Task<IActionResult> DeleteSchedule(int id)
        {
            var schedule = await _context.Schedules
                .Include(s => s.Assignments)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (schedule == null)
                return NotFound();

            _context.Schedules.Remove(schedule);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
