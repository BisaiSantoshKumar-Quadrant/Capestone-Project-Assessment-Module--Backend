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

            if (string.IsNullOrWhiteSpace(dto.CategoryName))

                return "CategoryName is required.";

            var questions = new List<Question>();

            try

            {

                using var reader = new StreamReader(dto.File.OpenReadStream());

                var config = new CsvConfiguration(CultureInfo.InvariantCulture)

                {

                    HeaderValidated = null,

                    MissingFieldFound = null

                };

                using var csv = new CsvReader(reader, config);

                csv.Context.RegisterClassMap<ExamQuestionMap>();

                var csvQuestions = csv.GetRecords<ExamQuestionCsv>().ToList();

                int examId = 0;

                if (dto.CategoryName != "All")

                {

                    int categoryId = _dbContext.Categories

                        .Where(c => c.CategoryName == dto.CategoryName)

                        .Select(c => c.CategoryId)

                        .FirstOrDefault();

                    if (categoryId == 0)

                        return $"Category '{dto.CategoryName}' not found.";

                    var newExam = new AssessmentDescription

                    {

                        Topic = dto.Topic,

                        Description = dto.Description,

                        DateConducted = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,

                            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),

                        TotalQuestions = csvQuestions.Count(),

                        TimeLimit = dto.ExamDuration,

                        PassPercentage = dto.PassPercentage,

                        CategoryId = categoryId

                    };

                    _dbContext.Assessments.Add(newExam);

                    await _dbContext.SaveChangesAsync();

                    examId = newExam.AssessmentID;

                    var userRole = await _dbContext.Roles

                        .FirstOrDefaultAsync(r => r.RoleName == "User");

                    if (userRole == null)

                        return "User role not found. Please ensure roles are seeded.";

                    int userRoleId = userRole.RoleID;

                    var employees = await _dbContext.Employees

                        .Where(e => e.RoleID == userRoleId && e.CategoryId == categoryId)

                        .ToListAsync();

                    if (!employees.Any())

                    {

                        Console.WriteLine($"No employees found with role 'User' in category '{dto.CategoryName}'.");

                    }
                    else
                    {

                        var assessmentScores = employees.Select(emp => new AssessmentScore
                        {
                            EmployeeId = emp.EmployeeId,
                            AssessmentID = examId,
                            Score = 0,
                            IsTaken = false,
                            CategoryId = categoryId // Still needed for AssessmentScore
                        }).ToList();

                        // Bulk insert scores
                        await _dbContext.AssessmentScores.AddRangeAsync(assessmentScores);
                        await _dbContext.SaveChangesAsync();
                    }
                }

                else
                {

                    var newExam = new AssessmentDescription

                    {

                        Topic = dto.Topic,

                        Description = dto.Description,

                        DateConducted = TimeZoneInfo.ConvertTimeFromUtc(

                           DateTime.UtcNow,

                           TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")),

                        TotalQuestions = csvQuestions.Count(),

                        TimeLimit = dto.ExamDuration,

                        PassPercentage = dto.PassPercentage,

                        CategoryId = null
                    };

                    _dbContext.Assessments.Add(newExam);

                    await _dbContext.SaveChangesAsync();

                    examId = newExam.AssessmentID;

                    var userRole = await _dbContext.Roles

                      .FirstOrDefaultAsync(r => r.RoleName == "User");

                    if (userRole == null)

                        return "User role not found. Please ensure roles are seeded.";

                    int userRoleId = userRole.RoleID;

                    var employees = await _dbContext.Employees

                     .Where(e => e.RoleID == userRoleId)

                     .ToListAsync();

                    if (!employees.Any())

                    {

                        Console.WriteLine("No employees found with role 'User'.");

                    }

                    else

                    {

                        var assessmentScores = employees.Select(emp => new AssessmentScore
                        {
                            EmployeeId = emp.EmployeeId,
                            AssessmentID = examId,
                            Score = 0,
                            IsTaken = false,
                            CategoryId = emp.CategoryId // Still needed for AssessmentScore
                        }).ToList();

                        // Bulk insert scores
                        await _dbContext.AssessmentScores.AddRangeAsync(assessmentScores);
                        await _dbContext.SaveChangesAsync();

                    }

                }

                questions.AddRange(csvQuestions.Select(q => new Question

                {

                    ExamID = examId,

                    QuestionText = q.Question,

                    OptionA = q.OptionA,

                    OptionB = q.OptionB,

                    OptionC = q.OptionC,

                    OptionD = q.OptionD,

                    CorrectAns = q.Answer

                }));

                await _dbContext.Questions.AddRangeAsync(questions);

                await _dbContext.SaveChangesAsync();

            }

            catch (Exception ex)

            {

                return $"Error: {ex.Message}";

            }

            return $"{questions.Count} questions uploaded successfully!";

        }

    }

}