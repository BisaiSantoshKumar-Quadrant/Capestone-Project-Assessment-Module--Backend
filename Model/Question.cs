using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace QAssessment_project.Model
{
    public class Question
    {
        [Key]
        public int QuestionID { get; set; }

        // Foreign Key - Assessment
        public int ExamID { get; set; }
        [ForeignKey("ExamID")]
        public virtual AssessmentDescription Assessment { get; set; }

        [Required]
        public string QuestionText { get; set; }

        [Required]
        public string OptionA { get; set; }

        [Required]
        public string OptionB { get; set; }

        [Required]
        public string OptionC { get; set; }

        [Required]
        public string OptionD { get; set; }

        [Required]
        public string CorrectAns { get; set; }

        public virtual ICollection<EmployeeResponse> EmployeeResponses { get; set; }
    }

}
