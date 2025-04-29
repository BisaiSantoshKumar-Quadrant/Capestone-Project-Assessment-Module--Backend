namespace QAssessment_project.DTO
{
    public class AssessmentScoreDTO
    {
        public int Id { get; set; }
        public int EmployeeId { get; set; } // Reference to Employee
        public int AssessmentID { get; set; } // Reference to Assessment
        public int Score { get; set; }
        public DateTime DateTaken { get; set; }

        public int ReattemptCount { get; set; }
    }

}
