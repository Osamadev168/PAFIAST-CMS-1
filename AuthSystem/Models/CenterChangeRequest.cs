namespace AuthSystem.Models
{
    public class CenterChangeRequest
    {

        public int Id { get; set; }
        public string UserId { get; set; }
        public int TestId { get; set; }
        public int DesiredCalendarId { get; set; }
        public bool Approved { get; set; }
    }
}
