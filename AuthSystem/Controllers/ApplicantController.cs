using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    [Authorize]
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

            var userId = user?.Id;
            var applications = _test.TestApplications.Include(d => d.Test.TestDetails).Include(w => w.Calendar).Where(w => w.UserId == userId).ToList();
            return View(applications);
        }

        public async Task<IActionResult> Results()
        {
            try
            {
                var user = await _userManager.GetUserAsync(User);

                var userId = user?.Id;
                var applications = _test.TestApplications.Include(d => d.Test.TestDetails).Include(a => a.User).Include(w => w.Calendar).Where(w => w.UserId == userId && w.HasFinished == true).ToList();

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

                var userId = user?.Id;
                var result = _test.Results.Where(r => r.AttemptedBy == userId && r.TestId == testId && r.CalendarId == calendarId).FirstOrDefault()?.Score;
                ViewBag.Score = result;
                return View();
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        public async Task<IActionResult> PayFee()
        {
            var user = await _userManager.GetUserAsync(User);

            var userId = user?.Id;
            var today = DateOnly.FromDateTime(DateTime.UtcNow.Date);

            var applications = _test.TestApplications
                .Include(d => d.Test.TestDetails)
                .Include(w => w.Calendar)
                .Where(w => w.UserId == userId)
                .OrderByDescending(t => t.Id)
                .ToList();
            return View(applications);
        }

        public IActionResult Dashboard()
        {
            try
            {
                return View();
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        [HttpGet]
        public IActionResult TestResult(int resultId)
        {
            try
            {
                var result = _test.Results.Where(r => r.ResultId == resultId).FirstOrDefaultAsync();
                if (result.Result == null)
                {
                    return Json(new { Error = "Result Not Found!" });
                }
                return Json(result.Result);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }

        [Authorize(Roles = "Super Admin , Admin")]
        public async Task<IActionResult> ViewAllApplications(string userId)
        {
            try
            {
                List<TestApplication> allApplications = _test.TestApplications.Where(a => a.UserId == userId && a.IsPaid == true).Include(a => a.Test).ToList();
                ApplicationUser user = await _userManager.FindByIdAsync(userId);
                if (user != null)
                {
                    string applicantName = $"{user.FirstName + " " + user.LastName}";
                    ViewBag.ApplicantName = applicantName;
                }
                return View(allApplications);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }
    }
}