using QAssessment_project.DTO;

namespace QAssessment_project.Services
{
    public interface IAuthService
    {
        Task<string> RegisterAsync(RegisterDTO model);
        Task<LoginResultDTO> LoginAsync(LoginDTO model);
        //Task<string> DeleteUserAsync(string email);

        Task<(bool Success, string Message)> ForgotPasswordAsync(string email);
        Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDTO request);
    }
}
