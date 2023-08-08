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
    }
}