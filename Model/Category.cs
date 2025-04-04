using System.ComponentModel.DataAnnotations;

namespace QAssessment_project.Model
{
    public class Category
    {
        [Key]
        public int CategoryId { get; set; }

        [Required]
        public string CategoryName { get; set; } = string.Empty;
        public virtual ICollection<AssessmentDescription> AssessmentDescriptions { get; set; } = new List<AssessmentDescription>();


        public virtual ICollection<Employee> Employees { get; set; } = new List<Employee>();
        public virtual ICollection<AssessmentScore> AssessmentScores { get; set; } = new List<AssessmentScore>();
    }
}