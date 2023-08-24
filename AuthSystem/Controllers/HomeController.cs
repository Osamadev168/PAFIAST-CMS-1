using AuthSystem.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace AuthSystem.Controllers
{
    public class HomeController : Controller
    {
        public IActionResult Index()
        {
            if(User.Identity.IsAuthenticated)
            {

                return RedirectToAction("Dashboard", "Home");
            }
            else
            {
                return View();
            }
           
        }
        public IActionResult Dashboard()
        {
            return View();
        }
        public IActionResult Applicant_Dashboard()
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
    }
}