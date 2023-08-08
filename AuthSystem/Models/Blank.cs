using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AuthSystem.Models
{
    public class Blank
    {
        [Key]
        public int Id { get; set; }

        public string Statement { get; set; }
        public string Answer { get; set; }
        public string Difficulty { get; set; }
        public string? Synonym1 { get; set; }
        public string? Synonym2 { get; set; }
        public string? Synonym3 { get; set; }

        [ForeignKey("Subject")]
        public int SubjectId { get; set; }

        public virtual Subject Subject { get; set; }
    }
}