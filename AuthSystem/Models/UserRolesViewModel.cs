using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class UserRolesViewModel
    {
        [Key]
        public string UserId { get; set; }

        public string UserName { get; set; }
        public List<IdentityRole> Roles { get; set; }
        public List<string> UserRoles { get; set; }
        public string[]? SelectedRoles { get; set; }
    }
}