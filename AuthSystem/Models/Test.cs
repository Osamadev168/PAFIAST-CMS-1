using DCMS.Lib.Data.Abstractions.Attributes;
using Microsoft.Build.Framework;

namespace AuthSystem.Models
{
    public class Test
    {
        [Key]

        public int Id { get; set; }

        [Required]
        public string TestName { get; set; }
        public string CreatedBy { get; set; }
        public int Duration { get; set; }
        public int TimeSpan { get; set; }
        public List<Test> TestList { get; set; }
        public List<Subject> Subjects { get; set; }
        public List<TestDetail> TestDetails { get; set; }
        public List<TestCalenders> TestCalenders { get; set; }
    }
}
