using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace QAssessment_project.Model
{
    public class AssessmentScore
    {
        [Key]
        public int Id { get; set; }

        public bool IsTaken{ get; set; } 
        // Foreign Key - Employee
        public int EmployeeId { get; set; }


        
        [ForeignKey("EmployeeId")]
        public virtual Employee Employee { get; set; }

        // Foreign Key - Assessment
        public int AssessmentID { get; set; }
        
        [ForeignKey("AssessmentID")]
        public virtual AssessmentDescription Assessment { get; set; }

        public int Score { get; set; } = 0; // Default score to 0

        public DateTime DateTaken { get; set; } = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));// Default to current time

     
    }
}
