namespace QAssessment_project.DTO
{
    public class QuestionDTO
    {
        public int AssesmentId { get; set; }
        public int QuestionID { get; set; }
      // Reference to Assessment
        public string QuestionText { get; set; }
        public List<string> Options { get; set; } // List instead of separate options

    }

}
