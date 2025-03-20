using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using QAssessment_project.DTO;

namespace QAssessment_project.Services
{
    public interface IQuestionUploadService
    {
        Task<string> UploadQuestionsAsync(QuestionUploadDto dto);
    }
}
