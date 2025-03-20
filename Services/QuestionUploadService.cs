using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using QAssessment_project.Data;
using QAssessment_project.DTO;
using QAssessment_project.Model;
using System;

namespace QAssessment_project.Services
{
    public class QuestionUploadService : IQuestionUploadService
    {
        private readonly ApplicationDbContext _dbContext;

        public QuestionUploadService(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<string> UploadQuestionsAsync(QuestionUploadDto dto)
        {
            
            if (dto.File == null || dto.File.Length == 0)
                return "File is empty.";

            var questions = new List<Question>();

            try
            {
                using var reader = new StreamReader(dto.File.OpenReadStream());
                var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                {
                    HeaderValidated = null, // Disable header validation
                    MissingFieldFound = null // Ignore missing fields
                };

                using var csv = new CsvReader(reader, config);
                csv.Context.RegisterClassMap<ExamQuestionMap>();
                var csvQuestions = csv.GetRecords<ExamQuestionCsv>().ToList();

                // Use topic and description from the DTO
                var newExam = new AssessmentDescription
                {
                    Topic = dto.Topic,
                    Description = dto.Description,
                    DateConducted = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),
                    TotalQuestions = csvQuestions.Count(),
                    TimeLimit=dto.ExamDuration
                };
                _dbContext.Assessments.Add(newExam);
                await _dbContext.SaveChangesAsync();
                int examId = newExam.AssessmentID;

                // Get all employees with RoleID = 1
                ICollection<Employee> employees1 = _dbContext.Employees
                    .Where(n => n.RoleID == 1)
                    .ToList();

                AssessmentDescription assessmentDescription = _dbContext.Assessments
                    .OrderByDescending(a => a.AssessmentID)
                    .FirstOrDefault();

                List<AssessmentScore> assessmentScores = new List<AssessmentScore>();

                foreach (Employee emp in employees1)
                {
                    AssessmentScore assessmentScore = new AssessmentScore
                    {
                        EmployeeId = emp.EmployeeId,
                        AssessmentID = assessmentDescription.AssessmentID,
                        Score = 0,
                        IsTaken = false,
                       
                    };

                    assessmentScores.Add(assessmentScore);
                }

                // ✅ Bulk insert scores in one go
                await _dbContext.AssessmentScores.AddRangeAsync(assessmentScores);
                await _dbContext.SaveChangesAsync();

                foreach (var q in csvQuestions)
                {
                    questions.Add(new Question
                    {
                        ExamID = examId,
                        QuestionText = q.Question,
                        OptionA = q.OptionA,
                        OptionB = q.OptionB,
                        OptionC = q.OptionC,
                        OptionD = q.OptionD,
                        CorrectAns = q.Answer
                    });
                }
            }
            catch (Exception ex)
            {
                return $"Error parsing CSV file: {ex.Message}";
            }

            try
            {
                await _dbContext.Questions.AddRangeAsync(questions);
                await _dbContext.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                return $"Database error: {ex.Message}";
            }

            return $"{questions.Count} questions uploaded successfully!";
        }
    }
}
