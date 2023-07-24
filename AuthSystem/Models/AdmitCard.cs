using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class AdmitCard
    {
        [Key]
        public int Id { get; set; }
        public string TestName { get; set; }
        public string ApplicantName { get; set; }
        public DateOnly Date { get; set; }
        public TimeOnly StartTime { get; set; }
        public TimeOnly EndTime { get; set; }
        public string TestCenterName { get; set; }
        public string TestCenterLocation { get; set; }
    }
}
