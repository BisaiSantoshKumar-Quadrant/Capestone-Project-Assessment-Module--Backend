using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAssessment_project.Model
{
    public class AssessmentDescription
    {
        [Key]
        public int AssessmentID { get; set; }

        [Required, StringLength(255)]
        public string Topic { get; set; } = string.Empty; // Default empty string

        [Required]
        public string Description { get; set; } = string.Empty; // Default empty string

        public DateTime DateConducted { get; set; }

        public int TotalQuestions { get; set; }

        public int  TimeLimit { get; set; }

       



        public virtual ICollection<Question> Questions { get; set; } = new List<Question>(); // Initialize to avoid null
        public virtual ICollection<EmployeeResponse> EmployeeResponses { get; set; } = new List<EmployeeResponse>(); // Initialize to avoid null
        public virtual ICollection<AssessmentScore> AssessmentScores { get; set; } = new List<AssessmentScore>(); // Initialize to avoid null
    }
}
