using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class TestCenters
    {
        [Key]
        public int Id { get; set; }
        public string TestCenterName { get; set; }
        public string TestCenterLocation { get; set; }
        public int Capacity { get; set; }
        public List<TestCenters> TestCentersList { get; set; }
    }
}
