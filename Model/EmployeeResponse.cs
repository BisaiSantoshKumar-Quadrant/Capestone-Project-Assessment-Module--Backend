using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAssessment_project.Model
{
    public class EmployeeResponse
    {
        [Key]
        public int Id { get; set; }
    

        // Foreign Key - Employee
        public int EmployeeID { get; set; }

        [ForeignKey("EmployeeID")]
        public virtual Employee Employee { get; set; }

        // Foreign Key - Assessment
        public int AssessmentID { get; set; }

        [ForeignKey("AssessmentID")]
        public virtual AssessmentDescription Assessment { get; set; }

        // Foreign Key - Question
        public int QuestionID { get; set; }

        [ForeignKey("QuestionID")]
        public virtual Question Question { get; set; }

        [Required]
        [StringLength(255)]
        public string SelectedOption { get; set; } = string.Empty; // Default empty string
    }
}
