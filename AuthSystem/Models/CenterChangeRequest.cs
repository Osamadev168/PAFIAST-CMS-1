namespace AuthSystem.Models
{
    public class CenterChangeRequest
    {

        public int Id { get; set; }

        public string UserId { get; set; }
        public int TestId { get; set; }
        public int PreCalendarId { get; set; }
        public int DesiredCalendarId { get; set; }
        public bool Approved { get; set; }
        public string PreCalendarToken { get; set; }
        public string DesiredCalendarToken { get; set; }
        public string PreCenterName { get; set; }
        public string DesiredCenterName { get; set; }
    }
}
