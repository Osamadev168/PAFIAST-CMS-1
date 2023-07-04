using AuthSystem.Areas.Identity.Data;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class TestApplication
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TestId { get; set; }
        public int? CalendarId { get; set; }
        public DateTime SelectionTime { get; set; }
        public string? CalenderToken { get; set; }
        public bool? IsPaid { get; set; }
        public bool? IsVerified { get; set; }
        public int? VoucherNumber { get; set; }
        public string? BankName { get; set; }
        public string? BranchCode { get; set; }
        public string? BranchName { get; set; }
        public string? VoucherPhotoPath { get; set; }

        public bool? isRejected { get; set; }

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }
        [ForeignKey("TestId")]
        public Test Test { get; set; }
        [ForeignKey("TestCalendar")]
        public TestCalenders Calendar { get; set; }
    }
}
