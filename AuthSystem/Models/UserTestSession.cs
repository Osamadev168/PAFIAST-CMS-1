namespace AuthSystem.Models
{
    public class UserTestSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int TestId { get; set; }
        public DateTime StartTime { get; set; }
    }
}
