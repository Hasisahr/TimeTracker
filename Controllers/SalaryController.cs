using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeTracker.DAL;
using TimeTracker.Model;

namespace TimeTracker.Web.Controllers
{
    public class SalaryController : Controller
    {
        private readonly TimeTrackerManagerDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public SalaryController(TimeTrackerManagerDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [HttpGet]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "UserName");

            return View();
        }

        [HttpPost]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public IActionResult RedirectToReview(string userId, int month, int year)
        {
            return RedirectToAction("ReviewSalary", new { userId, month, year });
        }

        [HttpGet]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> ReviewSalary(string userId, int month, int year)
        {
            var shifts = await _context.Shifts
                .Include(s => s.IdentityUser) 
                .Where(s => s.IdentityUserId == userId &&
                            s.StartTime.Month == month &&
                            s.StartTime.Year == year)
                .ToListAsync();


            var totalSalary = shifts.Sum(s => s.Salary);
            ViewBag.TotalSalary = totalSalary;
            ViewBag.UserId = userId;
            ViewBag.Month = month;
            ViewBag.Year = year;
            ViewBag.isPrivilaged = true;
            return View(shifts);
        }

        [HttpPost]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> LockSalary(string userId, int month, int year, decimal totalSalary)
        {
            var shifts = await _context.Shifts
                .Where(s => s.IdentityUserId == userId &&
                            s.StartTime.Month == month &&
                            s.StartTime.Year == year)
                .ToListAsync();

            if (!shifts.Any())
            {
                TempData["Message"] = "Nema smjena za zaključati.";
                return View();
            }

            var salaryRecord = new Salary
            {
                IdentityUserId = userId,
                Month = month,
                Year = year,
                TotalSalary = totalSalary,
            };

            _context.Salaries.Add(salaryRecord);

            _context.Shifts.RemoveRange(shifts);

            await _context.SaveChangesAsync();

            TempData["Message"] = $"Plaća za {month}/{year} zaključana. Ukupno: {totalSalary} €";
            return View();
        }

        [Authorize(Roles = "Worker")]
        [HttpGet]
        public async Task<IActionResult> UserSalaries()
        {
            var userId = _userManager.GetUserId(User);

            var salaries = await _context.Salaries
                .Where(s => s.IdentityUserId == userId)
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .ToListAsync();

            return View(salaries);
        }
    }
}
