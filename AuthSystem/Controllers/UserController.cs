using AuthSystem.Areas.Identity.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _hostingEnvironment;

        public UserController(UserManager<ApplicationUser> userManager , IWebHostEnvironment hostingEnvironment)
        {

            _userManager = userManager;
            _hostingEnvironment = hostingEnvironment;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Profile() {


            return View();
        
        }
        [HttpPost]
        public async Task<IActionResult> EditProfile(string firstName , string lastName , string dob , string country , string province , string city ,  IFormFile PP , string fatherName , string address)
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

            if (PP != null) {
                string fileName = Guid.NewGuid().ToString() + Path.GetExtension(PP.FileName);

                string uploadFolder = Path.Combine(_hostingEnvironment.WebRootPath, "ProfilePhotos");
                Directory.CreateDirectory(uploadFolder);

                string filePath = Path.Combine(uploadFolder, fileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await PP.CopyToAsync(fileStream);
                }
                user.ProfilePhoto = Path.Combine("\\ProfilePhotos", fileName);
                // Update other custom fields
            }

            var result = await _userManager.UpdateAsync(user);
                if (result.Succeeded)
                {
                    // Profile update successful, redirect to a success page or perform other actions
                    return RedirectToAction("Profile");
                }

                // Handle the case where the update failed (e.g., display error messages)
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            

            // If the model is not valid, return to the view with the validation errors
            return RedirectToAction("Profile");
        }

    }
}
