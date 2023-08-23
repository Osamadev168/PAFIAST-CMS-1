namespace AuthSystem.Models
{
    public class UserTestSession
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public DateTime StartTime { get; set; }

        public int ApplicationId { get; set; }
    }
}