using DCMS.Lib.Data.Abstractions.Attributes;

namespace AuthSystem.Models
{
    public class UserResponses
    {
        [Key]
        public int Id { get; set; }
        public int QuestionId { get; set; }
        public string Response { get; set; }
    }
}
