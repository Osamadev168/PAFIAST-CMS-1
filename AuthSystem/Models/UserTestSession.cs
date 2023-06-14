namespace AuthSystem.Models
{
    public class UserTestSession
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public int TestId { get; set; }
        public DateTime StartTime { get; set; }
        public int CalendarId { get; set; }
    }
}
