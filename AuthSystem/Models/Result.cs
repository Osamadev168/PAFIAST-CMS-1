using DCMS.Lib.Data.Abstractions.Attributes;

namespace AuthSystem.Models
{
    public class Result
    {
        [Key]
        public int ResultId { get; set; }

        public int Score { get; set; }

        public int TestId { get; set; }

        public int CalendarId { get; set; }
        public string AttemptedBy { get; set; }
    }
}