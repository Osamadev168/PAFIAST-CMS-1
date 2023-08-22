using AuthSystem.Areas.Identity.Data;
using AuthSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AuthSystem.Controllers
{
    [Authorize(Roles = "Super Admin")]
    public class RolesController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public RolesController(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public IActionResult Index()
        {
            var users = _userManager.Users.ToList();
            return View(users);
        }

        public IActionResult Users(string userId)
        {
            var user = _userManager.FindByIdAsync(userId).Result;
            var roles = _roleManager.Roles.ToList();
            var userRoles = _userManager.GetRolesAsync(user).Result;

            var model = new UserRolesViewModel
            {
                UserId = userId,
                UserName = user.UserName,
                Roles = roles,
                UserRoles = (List<string>)userRoles
            };

            return View(model);
        }

        public async Task<IActionResult> AssignRole(UserRolesViewModel model)
        {
            var user = await _userManager.FindByIdAsync(model.UserId);

            if (user != null)
            {
                var userRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, userRoles);
                var selectedRoles = model.SelectedRoles ?? new string[] { };
                await _userManager.AddToRolesAsync(user, selectedRoles);
            }

            return RedirectToAction("Users", new { userId = user.Id });
        }
    }
}