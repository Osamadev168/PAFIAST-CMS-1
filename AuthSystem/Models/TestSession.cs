namespace AuthSystem.Models
{
    public class TestSession
    {
        public int Id { get; set; }
        public string User { get; set; }
        public Test Test { get; set; }
        public List<MCQ> Questions { get; set; }
    }


}
