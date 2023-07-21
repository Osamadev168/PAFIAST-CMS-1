using AuthSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class TestApplication
    {
        public TestApplication()
        {
            IsPaid = false;
            IsVerified = false;
            IsRejected = false;
            IsPresent = false;
        }
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TestId { get; set; }
        public int? CalendarId { get; set; }
        public DateTime SelectionTime { get; set; }
        public string? CalenderToken { get; set; }
        public bool? IsPaid { get; set; } = false;
        public bool? IsVerified { get; set; } = false;
        public bool? HasChangedCenter { get; set; } = false;
      
        public int? VoucherNumber { get; set; }
        public string? BankName { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? VoucherPhotoPath { get; set; }

        public bool? IsPresent { get; set; } = false;

        public bool? IsRejected { get; set; } = false;

        public int? CalendarCode { get; set; }

        public bool? HasAttempted  {get; set;}

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("TestId")]
        public Test Test { get; set; }
        [ForeignKey("TestCalendar")]
        public TestCalenders Calendar { get; set; }
    }
}
