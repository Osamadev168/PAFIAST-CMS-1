using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    public class ApplicantController : Controller
    {
        private readonly AuthDbContext _test;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicantController(AuthDbContext test, UserManager<ApplicationUser> user)
        {
            _test = test;
            _userManager = user;
        }

        public async Task<IActionResult> Applications()
        {
            var user = await _userManager.GetUserAsync(User);

            var userId = user.Id;
            var applications = _test.TestApplications.Include(d => d.Test.TestDetails).Include(w => w.Calendar).Where(w => w.UserId == userId).ToList();
            return View(applications);
        }

        public async Task<IActionResult> Results()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var userId = user.Id;
                var applications = _test.TestApplications.Include(d => d.Test.TestDetails).Include(w => w.Calendar).Where(w => w.UserId == userId && w.HasFinished == true).ToList();

                return View(applications);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public async Task<IActionResult> ViewResult(int testId, int calendarId)
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var userId = user.Id;
                var result = _test.Results.Where(r => r.AttemptedBy == userId && r.TestId == testId && r.CalendarId == calendarId).FirstOrDefault().Score;
                ViewBag.Score = result;
                return View();
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }
    }
}