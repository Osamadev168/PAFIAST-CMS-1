using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class AssignedQuestions
    {
        [Key]
        public int Id { get; set; }

        public int UserId { get; set; }

        [ForeignKey("QuestionId")]
        public MCQ Question { get; set; }
        public int QuestionId { get; set; }

        [ForeignKey("TestDetailId")]
        public TestDetail TestDetail { get; set; }
        public int TestDetailId { get; set; }

        public string? UserResponse { get; set; }

    }
}
