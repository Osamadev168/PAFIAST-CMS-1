using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class Subject
    {
        public IEnumerable<Subject> Subjects { get; set; }
        [Key]

        public int SubjectId { get; set; }
        public string SubjectName { get; set; }
    }
}