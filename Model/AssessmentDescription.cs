using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAssessment_project.Model
{
    public class AssessmentDescription
    {
        [Key]
        public int AssessmentID { get; set; }

        [Required, StringLength(255)]
        public string Topic { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public DateTime DateConducted { get; set; }

        public int TotalQuestions { get; set; }

        public int TimeLimit { get; set; }

        public int PassPercentage { get; set; }
        public int? CategoryId {  get; set; }
        [ForeignKey("CategoryId")]
        public Category Category { get; set; }

        public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        public virtual ICollection<EmployeeResponse> EmployeeResponses { get; set; } = new List<EmployeeResponse>();
        public virtual ICollection<AssessmentScore> AssessmentScores { get; set; } = new List<AssessmentScore>();
    }
}