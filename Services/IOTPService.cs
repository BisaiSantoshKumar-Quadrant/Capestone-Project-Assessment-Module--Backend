namespace QAssessment_project.Services
{
    public interface IOTPService
    {
        string GenerateOTP(int length);
        Task<bool> VerifyOTPAsync(string email, string otp);
        Task SendOTPAsync(string email);
    }
}
