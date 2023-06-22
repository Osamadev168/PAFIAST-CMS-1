using AuthSystem.Areas.Identity.Data;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class UserCalendars
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TestId { get; set; }
        public int? CalendarId { get; set; }
        public DateTime SelectionTime { get; set; }
        public string? CalenderToken { get; set; }
        public bool? hasApplied { get; set; }
        // Navigation properties    
       
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; }

        [ForeignKey("TestId")]
        public Test Test { get; set; }

        [ForeignKey("TestCalendar")]
        public TestCalenders Calendar { get; set; }
    }
}
