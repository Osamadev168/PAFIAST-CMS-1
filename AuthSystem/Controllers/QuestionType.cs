using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    [Authorize]
    public class QuestionType : Controller
    {
        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult Index()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult CreationType()
        {
            return View();
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult CreationTypeFIB()
        {
            return View();
        }
    }
}