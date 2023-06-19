using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    public class CalendarController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AuthDbContext _test;
        public CalendarController(AuthDbContext test , UserManager<ApplicationUser> userManager)
        {

            _test = test;
            _userManager = userManager;
        }
        public async Task<IActionResult> Index()
        {
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Include(td => td.Test).ToList(),
            };

            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                viewModel.UserCalendars = _test.UserCalendars
                    .Where(uc => uc.UserId == user.Id)
                    .ToList();
            }

            return View(viewModel);
        }


        public async Task<IActionResult> SelectCalendar(int testId, int calendarId, string calendarToken)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return NotFound("User not found");
            }

            var userId = user.Id;

            var existingUserCalendar = _test.UserCalendars.FirstOrDefault(uc => uc.UserId == userId && uc.TestId == testId);

            if (existingUserCalendar != null)
            {
                existingUserCalendar.CalendarId = calendarId;
                existingUserCalendar.SelectionTime = DateTime.Now;
                existingUserCalendar.CalenderToken = calendarToken;
                _test.UserCalendars.Update(existingUserCalendar);
                _test.SaveChanges();
                return Conflict("Calendar Updated!");
            }



            _test.UserCalendars.Add(new UserCalendars
            {
                UserId = userId,
                TestId = testId,
                CalendarId = calendarId,
                SelectionTime = DateTime.Now,
                CalenderToken = calendarToken
            });

            _test.SaveChanges();

            return Ok("Applied Successfully!");
        }



        public async Task<IActionResult> UserCalendars()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
            {
                return Content("User not found");
            }

            var userId = user.Id;

            var userCalendars = _test.UserCalendars
                .Where(uc => uc.UserId == userId)
                .Include(uc => uc.Test)
                .Include(uc => uc.Calendar)
                .ToList();

            return View(userCalendars);
        }

    }
}
