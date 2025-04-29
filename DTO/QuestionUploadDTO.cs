using Microsoft.AspNetCore.Http;

namespace QAssessment_project.DTO
{


   
        public class QuestionUploadDto
        {
            public IFormFile File { get; set; }
            public string Topic { get; set; }
            public string Description { get; set; }

            public int ExamDuration { get; set; }

        public string CategoryName {  get; set; }   
        public int PassPercentage { get; set; }

        public int ReattemptCount { get; set; }

        public int QuestionConduct { get; set; }

    }
    

}
