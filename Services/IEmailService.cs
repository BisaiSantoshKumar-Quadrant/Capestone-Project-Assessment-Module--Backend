// Services/IEmailService.cs - Email Service Interface
namespace QAssessment_project.Services
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body);
    }
}