using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace AuthSystem.Models
{
    public class UserRolesViewModel
    {
        [Key]
        public string UserId { get; set; }
        public string UserName { get; set; }
        public List<IdentityRole> Roles { get; set; }
        [Key]
        public List<string> UserRoles { get; set; }
        [NotMapped]
        public string[]? SelectedRoles { get; set; }
    }
}