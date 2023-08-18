using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    [Authorize]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserController(UserManager<ApplicationUser> userManager, IWebHostEnvironment hostingEnvironment)
        {
            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile()
        {
            return View();
        }

        public async Task<IActionResult> EditProfile(string firstName, string lastName, string dob, string country, string province, string city, IFormFile PP, string fatherName, string address)
        {
            var user = await _userManager.GetUserAsync(User);
            user.FirstName = firstName;
            user.LastName = lastName;
            user.Dob = dob;
            user.Country = country;
            user.Province = province;
            user.City = city;
            user.FatherName = fatherName;
            user.Address = address;

            if (PP != null)
            {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(PP.FileName);

                string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "ProfilePhotos");
                Directory.CreateDirectory(uploadFolder);

                string filePath = Path.Combine(uploadFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PP.CopyToAsync(fileStream);
                }
                user.ProfilePhoto = Path.Combine("\\ProfilePhotos", fileName);
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                return RedirectToAction("Profile");
            }

            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }

            return RedirectToAction("Profile");
        }

        [Authorize(Roles = "Admin,Super Admin")]
        public IActionResult ViewUserInfo(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            if (user == null)
            {
                return Content("Error fetching user's data");
            }
            ViewBag.UserName = user.Email;
            ViewBag.FullName = user.FirstName + " " + user.LastName;
            ViewBag.FatherName = user.FatherName;
            ViewBag.Email = user.Email;
            ViewBag.PhoneNumber = user.PhoneNumber;
            ViewBag.Address = user.Address;
            ViewBag.City = user.City;
            ViewBag.Country = user.Country;
            ViewBag.Province = user.Province;
            ViewBag.Photo = user.ProfilePhoto;
            return View();
        }
    }
}