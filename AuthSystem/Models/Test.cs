using DCMS.Lib.Data.Abstractions.Attributes;

namespace AuthSystem.Models
{
    public class Test
    {
        [Key]
        public int Id { get; set; }

        public string TestName { get; set; }
        public string CreatedBy { get; set; }
        public int Duration { get; set; }
        public int TimeSpan { get; set; }

        public int SessionId { get; set; }
        public List<Test> TestList { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<TestDetail> TestDetails { get; set; }
        public List<TestCalenders> TestCalenders { get; set; }
        public List<TestApplication> TestApplications { get; set; }

        public Test()
        {
            TestApplications = new List<TestApplication>();
        }
    }
}