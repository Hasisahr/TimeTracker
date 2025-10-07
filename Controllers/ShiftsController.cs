using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TimeTracker.DAL;
using TimeTracker.Model;
using TimeTracker.Web.Models;

namespace TimeTracker.Web.Controllers
{
    [Authorize]
    public class ShiftsController : Controller
    {
        private readonly TimeTrackerManagerDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public ShiftsController(TimeTrackerManagerDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }


        [HttpGet]
        [Authorize(Roles = "Worker, Admin, ShiftLeader")]
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var roles = await _userManager.GetRolesAsync(await _userManager.GetUserAsync(User));
            var isPrivilaged = true;
            IQueryable<Shift> query = _context.Shifts;

            if (!roles.Contains("Admin") && !roles.Contains("ShiftLeader"))
            {
                query = query.Where(s => s.IdentityUserId == userId);
                isPrivilaged = false;
            }

            var shifts = await query
                .Include(s => s.IdentityUser)
                .ToListAsync();
            ViewBag.isPrivilaged = isPrivilaged;
            return View(shifts);
        }


        [Authorize(Roles = "Worker")]
        [HttpPost]
        public async Task<IActionResult> AddShift(Shift shift)
        {
            var userId = _userManager.GetUserId(base.User);
            var user = await _userManager.FindByIdAsync(userId);
            ModelState.Remove("IdentityUser");


            shift.IdentityUserId = userId;
            shift.IdentityUser = user;


            if (ModelState.IsValid)
            {
                shift.StartTime = shift.ShiftDate.Date.Add(shift.StartTime.TimeOfDay);
                shift.EndTime = shift.ShiftDate.Date.Add(shift.EndTime.TimeOfDay);

                shift.IsOvertime = shift.StartTime.DayOfWeek == DayOfWeek.Sunday;
                shift.calculateWageForShift();

                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(shift);
        }

        [HttpGet]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> AddShiftForUser()
        {
            var users = await _userManager.Users.ToListAsync();

            ViewBag.Users = new SelectList(users, "Id", "UserName");
            return View(new Shift());
        }


        [HttpPost]
        [Authorize(Roles = "ShiftLeader,Admin")]
        public async Task<IActionResult> AddShiftForUser(Shift shift)
        {
            ModelState.Remove("IdentityUser");

            if (ModelState.IsValid)
            {
                shift.StartTime = shift.ShiftDate.Date.Add(shift.StartTime.TimeOfDay);
                shift.EndTime = shift.ShiftDate.Date.Add(shift.EndTime.TimeOfDay);
                shift.IsOvertime = shift.StartTime.DayOfWeek == DayOfWeek.Sunday;
                shift.calculateWageForShift();

                var user = await _userManager.FindByIdAsync(shift.IdentityUserId);
                shift.IdentityUser = user;

                _context.Shifts.Add(shift);
                await _context.SaveChangesAsync();

                return RedirectToAction("Index");
            }

            var users = await _userManager.Users.ToListAsync();
            ViewBag.Users = new SelectList(users, "Id", "UserName", shift.IdentityUserId);
            return View(shift);
        }

        [ActionName(nameof(Edit))]
        public IActionResult Edit(int id)
        {
            var model = _context.Shifts.FirstOrDefault(c => c.Id == id);
            
            return View(model);
        }

        [HttpPost]
        [ActionName(nameof(Edit))]
        public async Task<IActionResult> EditPost(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null) return NotFound();
            ModelState.Remove("IdentityUser");

            var user = await _userManager.FindByIdAsync(shift.IdentityUserId);
            shift.IdentityUser = user;

            if (await TryUpdateModelAsync(shift))
            {

                shift.StartTime = shift.ShiftDate.Date.Add(shift.StartTime.TimeOfDay);
                shift.EndTime = shift.ShiftDate.Date.Add(shift.EndTime.TimeOfDay);
                shift.IsOvertime = shift.StartTime.DayOfWeek == DayOfWeek.Sunday;

                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(shift);
        }
        [HttpPost]
        public async Task<IActionResult> ListAjax(ShiftFilterModel filter = null)
        {
            var query = _context.Shifts.AsQueryable();

            if (filter.Date.HasValue)
            {
                query = query.Where(s => s.ShiftDate.Date == filter.Date.Value.Date);
            }

            if (filter.FromTime.HasValue)
            {
                query = query.Where(s => s.StartTime.TimeOfDay >= filter.FromTime.Value.TimeOfDay);
            }

            if (filter.ToTime.HasValue)
            {
                query = query.Where(s => s.EndTime.TimeOfDay <= filter.ToTime.Value.TimeOfDay);
            }

            if (filter.Position.HasValue)
            {
                query = query.Where(s => s.Position == filter.Position.Value);
            }

            var results = await query
                .Include(s => s.IdentityUser)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            ViewBag.isPrivilaged = User.IsInRole("ShiftLeader") || User.IsInRole("Admin");

            return PartialView("_ShiftTablePartial", results);
        }

        [HttpPost]
        [Authorize(Roles = "ShiftLeader,Admin, Worker")]
        public async Task<IActionResult> Delete(int id)
        {
            var shift = await _context.Shifts.FindAsync(id);
            if (shift == null)
            {
                return NotFound();
            }

            _context.Shifts.Remove(shift);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index");
        }
    }
}
