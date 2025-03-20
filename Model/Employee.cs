using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Data;

namespace QAssessment_project.Model
{
    public class Employee
    {
        [Key]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        public string Password { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public DateTime JoinedDate { get; set; } = DateTime.UtcNow;

        public string? ResetToken { get; set; } = string.Empty;
        public DateTime? ResetTimeExpires { get; set; } // Nullable DateTime






        // Foreign Key
        public int RoleID { get; set; }

        [ForeignKey("RoleID")]
        public virtual Role Role { get; set; }

        public virtual ICollection<EmployeeResponse> EmployeeResponses { get; set; } = new List<EmployeeResponse>();
        public virtual ICollection<AssessmentScore> AssessmentScores { get; set; } = new List<AssessmentScore>();
    }

}
