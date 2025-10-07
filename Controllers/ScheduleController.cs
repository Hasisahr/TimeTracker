using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeTracker.DAL;
using TimeTracker.Model;

namespace TimeTracker.Web.Controllers
{
    public class ScheduleController : Controller
    {
        private readonly TimeTrackerManagerDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        
        public ScheduleController(TimeTrackerManagerDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public IActionResult ScheduleInfo()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public IActionResult ScheduleInfo(Schedule schedule)
        {
            TempData["ScheduleDate"] = schedule.ScheduleDate.ToString("o");
            TempData["StartTime"] = schedule.StartTime.ToString();
            TempData["EndTime"] = schedule.EndTime.ToString();
            TempData["TicketsSold"] = schedule.TicketsSold;

            return RedirectToAction("AssignWorkers");
        }

        [HttpGet]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> AssignWorkers()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "UserName");

            var schedule = new Schedule
            {
                ScheduleDate = DateTime.Parse(TempData["ScheduleDate"].ToString()),
                StartTime = TimeSpan.Parse(TempData["StartTime"].ToString()),
                EndTime = TimeSpan.Parse(TempData["EndTime"].ToString()),
                TicketsSold = int.Parse(TempData["TicketsSold"].ToString()),
                Assignments = new List<ScheduleAssignment>()
            };

            int slotsPerPosition = schedule.TicketsSold > 200 ? 2 : 1;
            var positions = Enum.GetValues(typeof(Position)).Cast<Position>();

            foreach (var position in positions)
            {
                for (int i = 1; i <= slotsPerPosition; i++)
                {
                    schedule.Assignments.Add(new ScheduleAssignment
                    {
                        Position = position.ToString(),
                        SlotNumber = i
                    });
                }
            }

            return View(schedule);
        }

        [HttpPost]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> ConfirmSchedule(DateTime ScheduleDate, TimeSpan StartTime, TimeSpan EndTime, int TicketsSold, List<ScheduleAssignment> Assignments)
        {
            var userId = _userManager.GetUserId(User);

            var schedule = new Schedule
            {
                ScheduleDate = ScheduleDate,
                StartTime = StartTime,
                EndTime = EndTime,
                TicketsSold = TicketsSold,
                Assignments = new List<ScheduleAssignment>()
            };

            _context.Schedules.Add(schedule);
            await _context.SaveChangesAsync(); 

            foreach (var a in Assignments)
            {
                a.ScheduleId = schedule.Id; 
                schedule.Assignments.Add(a);
            }

            await _context.SaveChangesAsync();

            TempData["Message"] = "Raspored je uspješno spremljen.";
            return RedirectToAction("Schedule");
        }

        public async Task<IActionResult> Schedule()
        {
            var today = DateTime.Today;

            var schedules = await _context.Schedules
                .Where(s => s.ScheduleDate.Date >= today.Date)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Worker)
                .OrderBy(s => s.ScheduleDate)
                .ToListAsync();

            return View(schedules);

        }

        [HttpGet]
        [Authorize(Roles = "ShiftLeader,Admin, Worker")]
        public async Task<IActionResult> AllSchedules()
        {
            var today = DateTime.Today;

            var schedules = await _context.Schedules
                .Where(s => s.ScheduleDate.Date >= today.Date)
                .Include(s => s.Assignments)
                    .ThenInclude(a => a.Worker)
                .OrderBy(s => s.ScheduleDate)
                .ToListAsync();

            return View(schedules); 
        }

        [HttpGet]
        [Authorize(Roles = "Worker")]
        public async Task<IActionResult> MyAssignments()
        {
            var userId = _userManager.GetUserId(User);
            var today = DateTime.Today;

            var assignments = await _context.ScheduleAssignments
                .Where(a => a.WorkerId == userId && a.Schedule.ScheduleDate.Date >= today.Date)
                .Include(a => a.Schedule)
                .OrderBy(a => a.Schedule.ScheduleDate)
                .ThenBy(a => a.Position)
                .ToListAsync();

            return View(assignments);
        }
    }
}
