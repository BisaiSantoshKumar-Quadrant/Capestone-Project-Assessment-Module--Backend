namespace QAssessment_project.DTO
{
    public class AssessmentSubmissionDto
    {
        public int AssessmentId { get; set; }
        public int EmployeeId { get; set; }
        public List<EmployeeResponseDto> EmployeeResponses { get; set; }
    }
}
