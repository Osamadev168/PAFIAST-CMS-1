using AuthSystem.Areas.Identity.Data;
using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AuthSystem.Controllers
{
    public class ApplicationController : Controller
    {
        private readonly AuthDbContext _test;
        private readonly UserManager<ApplicationUser> _userManager;

        public ApplicationController(AuthDbContext test, UserManager<ApplicationUser> userManager)
        {
            _test = test;
            _userManager = userManager;
        }

        public IActionResult Index()
        {
            var viewModel = new Test
            {
                TestList = _test.Tests.OrderByDescending(q => q.Id).ToList(),
                Subjects = _test.Subjects.Include(td => td.Subjects).ToList(),
                TestDetails = _test.TestsDetail.Include(td => td.Test).ToList(),
                TestCalenders = _test.TestCalenders.Where(t => t.Date.Day >= DateTime.UtcNow.Day).Include(td => td.Test).Include(td => td.TestCenter).ToList(),
            };
            return View(viewModel);
        }

        public IActionResult Apply_Test(int testId)
        {
            string userId = _userManager.GetUserId(User);

            var testApplication = new TestApplication
            {
                TestId = testId,
                UserId = userId,
                SelectionTime = DateTime.Now,
            };
            _test.TestApplications.Add(testApplication);
            _test.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}