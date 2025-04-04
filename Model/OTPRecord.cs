using System.ComponentModel.DataAnnotations;

namespace QAssessment_project.Model
{
    public class OTPRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string OTP { get; set; } = string.Empty;

        [Required]
        public DateTime ExpiresAt { get; set; }
    }
}
