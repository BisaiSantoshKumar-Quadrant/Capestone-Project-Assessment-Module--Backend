namespace QAssessment_project.DTO
{
    public class AssessmentDescriptionDTO
    {
        public int AssessmentID { get; set; }
        public string Topic { get; set; }
        public string Description { get; set; }
        public DateTime DateConducted { get; set; }
        public int PassCriteria { get; set; }
    }
}