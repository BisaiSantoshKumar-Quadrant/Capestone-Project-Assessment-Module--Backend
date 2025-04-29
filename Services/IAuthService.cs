using QAssessment_project.DTO;

namespace QAssessment_project.Services
{
    public interface IAuthService
    {
        Task<bool> SendOTPForRegistrationAsync(string email);
        Task<bool> VerifyOTPForRegistrationAsync(string email, string otp);
        Task<string> RegisterAsync(RegisterDTO model);
        Task<string> ManagerRegisterAsync(ManagerRegisterDTO mdto);
        Task<LoginResultDTO> LoginAsync(LoginDTO model);
        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDTO request);

       // Task<(bool Success, string Message, LoginResultDTO LoginResult)> MicrosoftLoginAsync(string accessToken);

    }
}
