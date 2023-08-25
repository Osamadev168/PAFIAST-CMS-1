using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace AuthSystem.Controllers
{
    public class HomeController : Controller
    {
        private readonly AuthDbContext _test;

        public HomeController(AuthDbContext test)
        {
            _test = test;
        }

        public IActionResult Index()
        {
            if (User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Dashboard", "Home");
            }
            else
            {
                return View();
            }
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Dashboard()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public IActionResult GetTestsData()
        {
            try
            {
                var testsData = new
                {
                    TestList = _test.Tests.OrderByDescending(q => q.Id).ToArray(),
                    Subjects = _test.Subjects.Include(td => td.Subjects).ToArray(),
                    TestDetails = _test.TestsDetail.Include(td => td.Test).ToArray(),
                    TestCalenders = _test.TestCalenders
                                                      .Include(td => td.Test)
                                                      .Include(td => td.TestCenter)
                                                      .ToArray()
                };

                JsonSerializerOptions options = new JsonSerializerOptions
                {
                    ReferenceHandler = ReferenceHandler.Preserve
                    // Other serialization settings if needed
                };

                return Json(testsData, options);
            }
            catch (Exception e)
            {
                return Json(new { Error = e.Message });
            }
        }
    }
}