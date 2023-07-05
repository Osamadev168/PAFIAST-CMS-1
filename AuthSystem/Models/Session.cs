using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class Session
    {
        [Key]
        public int Id { get; set; }
        public string SessionName { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }

    }
}
