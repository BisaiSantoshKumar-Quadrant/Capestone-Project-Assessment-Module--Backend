namespace QAssessment_project.DTO
{
    public class UpdateQuestionDto
    {
        public int QuestionID { get; set; }
        public int ExamID { get; set; }
        public string QuestionText { get; set; }
        public string OptionA { get; set; }
        public string OptionB { get; set; }
        public string OptionC { get; set; }
        public string OptionD { get; set; }
        public string CorrectAns { get; set; }
    }
}
 