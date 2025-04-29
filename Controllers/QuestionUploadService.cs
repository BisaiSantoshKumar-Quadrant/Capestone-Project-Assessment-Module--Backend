using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using QAssessment_project.Services;
using QAssessment_project.DTO;

namespace QAssessment_project.Controllers
{
    [Route("api/question-upload")]
    [ApiController]
    public class QuestionUploadController : ControllerBase
    {
        private readonly IQuestionUploadService _questionUploadService;

        public QuestionUploadController(IQuestionUploadService questionUploadService)
        {
            _questionUploadService = questionUploadService;
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadQuestions([FromForm] QuestionUploadDto dto)
        {
            Console.WriteLine(dto.ExamDuration);
            Console.WriteLine(dto.QuestionConduct);
            var result = await _questionUploadService.UploadQuestionsAsync(dto);

            if (result.Contains("error", System.StringComparison.OrdinalIgnoreCase))
                return BadRequest(result);

            return Ok(new { message = result });
        }
    }
}
