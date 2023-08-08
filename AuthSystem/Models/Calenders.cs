using System.ComponentModel.DataAnnotations;

namespace AuthSystem.Models
{
    public class Calenders
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}