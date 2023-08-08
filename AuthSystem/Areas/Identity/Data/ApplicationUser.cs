using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Areas.Identity.Data;

public class ApplicationUser : IdentityUser
{
    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string FirstName { get; set; }

    [PersonalData]
    [Column(TypeName = "nvarchar(100)")]
    public string LastName { get; set; }

    [PersonalData]
    public string? Dob { get; set; }

    public string? Country { get; set; }
    public string? Province { get; set; }
    public string? City { get; set; }

    public string? ProfilePhoto { get; set; }

    public string? FatherName { get; set; }

    public string? Address { get; set; }

    public string CNIC { get; set; }
}