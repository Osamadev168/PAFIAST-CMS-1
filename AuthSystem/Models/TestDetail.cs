using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class TestDetail
    {
        [Key]
        public int TDId { get; set; }

        [ForeignKey("Test")]
        public int TestId { get; set; }
        public Test Test { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }
        public Subject Subject { get; set; }

        public int Percentage { get; set; }

        public int Easy { get; set; }

        public int Medium { get; set; }
        public int Hard { get; set; }

    }
}
