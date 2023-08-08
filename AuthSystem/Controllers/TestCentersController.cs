using AuthSystem.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    public class TestCentersController : Controller
    {
        private readonly AuthDbContext _test;

        public TestCentersController(AuthDbContext test)
        {
            _test = test;
        }

        public IActionResult ViewTestCenters()
        {
            var testCenters = _test.TestCenters.ToList();
            return View(testCenters);
        }

        public IActionResult Manage()
        {
            return View();
        }

        public IActionResult Create(string centerName, string centerLocation, int capacity)
        {
            var testCenter = new TestCenters
            {
                TestCenterName = centerName,
                TestCenterLocation = centerLocation,
                Capacity = capacity,
            };
            _test.TestCenters.Add(testCenter);
            _test.SaveChanges();
            return RedirectToAction("Manage");
        }
    }
}