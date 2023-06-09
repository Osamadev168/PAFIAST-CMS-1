using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class TestCalenders
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("Test")]
        public int TestId { get; set; }
        public Test Test { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string CalendarToken { get; set; }
    }
}
