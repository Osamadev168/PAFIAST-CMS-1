using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    public class QuestionType : Controller
    {
        [Authorize]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult CreationType()
        {
            return View();
        }

        public IActionResult CreationTypeFIB()
        {
            return View();
        }
    }
}