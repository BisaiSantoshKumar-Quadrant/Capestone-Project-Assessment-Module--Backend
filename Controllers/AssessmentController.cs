using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query.SqlExpressions;
using QAssessment_project.Data;
using QAssessment_project.DTO;
using QAssessment_project.Model;
using QAssessment_project.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QAssessment_project.Controllers
{
    [Route("api/assessment")]
    [ApiController]
   [Authorize] // Ensures only authenticated users access these routes
    public class AssessmentController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtService _jwtService;

        public AssessmentController(ApplicationDbContext context, JwtService jwtService)
        {
            _context = context;
            _jwtService = jwtService;
        }
        [HttpGet("user-assessments/{employeeId}")]
        public async Task<IActionResult> GetUserAssessments(int employeeId)
        {
            try
            {
                var pendingAssessments = await _context.AssessmentScores
                    .Where(a => a.EmployeeId == employeeId && !a.IsTaken)
                    .Select(a => a.AssessmentID)
                    .ToListAsync();

                var availableTests = await _context.Assessments
                    .Where(a => pendingAssessments.Contains(a.AssessmentID))
                    .Select(a => new
                    {
                        assessmentID = a.AssessmentID,
                        topic = a.Topic,
                        description = a.Description,
                        timeLimit=a.TimeLimit

                    })
                    .ToListAsync();

                return Ok(new { availableTests });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching assessments", error = ex.Message });
            }
        }

        [HttpGet("completed-assessments/{employeeId}")]
        public async Task<IActionResult> GetCompletedAssessments(int employeeId)
{
    try
    {
        var completedTests = await _context.AssessmentScores
            .Where(a => a.EmployeeId == employeeId && a.IsTaken) // Get completed tests
            .Join(_context.Assessments,  // Join with Assessments table
                score => score.AssessmentID,
                assessment => assessment.AssessmentID,
                (score, assessment) => new
                {
                    assessmentID = assessment.AssessmentID,
                    topic = assessment.Topic,
                    description = assessment.Description,
                    questionsConduct = assessment.QuestionConduct,
                    maxReattempts = assessment.ReattemptCount,  // Rename to clarify this is the maximum
                    currentAttempts = score.AttemptCount,       // Add current attempt count from AssessmentScores
                    score = score.Score,
                    passingPercentage = assessment.PassPercentage,
                   // totalQuestions = assessment.TotalQuestions,
                  
                    dateTaken = score.DateTaken
                })
            .ToListAsync();
 
        return Ok(new { completedTests });
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "An error occurred while fetching completed assessments", error = ex.Message });
    }
 
}

        [HttpGet("questions/{assessmentId}")]
        public async Task<IActionResult> GetQuestions(int assessmentId)
        {
            try
            {
                // 🔐 Extract Employee ID from JWT Token
                int employeeId = _jwtService.GetEmployeeIdFromToken();

                // ✅ Check if the exam is assigned to the user
                var examScore = await _context.AssessmentScores
                    .FirstOrDefaultAsync(s => s.EmployeeId == employeeId && s.AssessmentID == assessmentId);

                if (examScore == null)
                {
                    return NotFound(new { message = "This exam does not exist or is not assigned to you." });
                }

                // ✅ Prevent retaking completed exams if passed
                if (examScore.IsTaken && examScore.Status)
                {
                    return BadRequest(new
                    {
                        message = "You have already passed this exam.",
                        score = examScore.Score,
                        totalQuestions = await _context.Questions.CountAsync(q => q.ExamID == assessmentId),
                        percentage = (examScore.Score * 100) / await _context.Questions.CountAsync(q => q.ExamID == assessmentId),
                        dateTaken = examScore.DateTaken,
                        topic = await _context.Assessments.Where(a => a.AssessmentID == assessmentId).Select(a => a.Topic).FirstOrDefaultAsync(),
                        description = await _context.Assessments.Where(a => a.AssessmentID == assessmentId).Select(a => a.Description).FirstOrDefaultAsync()
                    });
                }

                // Retrieve QuestionConduct and other fields
                var assessment = await _context.Assessments
                    .Where(a => a.AssessmentID == assessmentId)
                    .Select(a => new { a.QuestionConduct, a.Topic, a.TimeLimit, a.PassPercentage })
                    .FirstOrDefaultAsync();

                if (assessment == null)
                {
                    return NotFound(new { message = "Assessment not found." });
                }

                // Validate QuestionConduct
                int questionCount = assessment.QuestionConduct;
                if (questionCount <= 0)
                {
                    return BadRequest(new { message = "QuestionConduct must be a positive integer." });
                }

                // Check if QuestionConduct exceeds total questions
                int totalQuestions = await _context.Questions.CountAsync(q => q.ExamID == assessmentId);
                if (questionCount > totalQuestions)
                {
                    return BadRequest(new { message = $"QuestionConduct ({questionCount}) exceeds total questions ({totalQuestions})." });
                }

                // Select random questions based on QuestionConduct count
                var questions = await _context.Questions
         .Where(q => q.ExamID == assessmentId)
         .OrderBy(q => Guid.NewGuid()) // ✅ EF can translate this to SQL's NEWID()
         .Take(questionCount)
         .Select(q => new QuestionDTO
         {
             QuestionID = q.QuestionID,
             AssesmentId = q.ExamID,
             QuestionText = q.QuestionText,
             Options = new List<string> { q.OptionA, q.OptionB, q.OptionC, q.OptionD }
         })
         .ToListAsync();

                if (!questions.Any())
                {
                    return NotFound(new { message = "No questions found for this assessment." });
                }

                return Ok(new
                {
                    questions,
                    duration = assessment.TimeLimit,
                    topic = assessment.Topic,
                    passingPercentage = assessment.PassPercentage // Map PassCriteria to passingPercentage in response
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving questions.", error = ex.Message });
            }
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitAssessment([FromBody] AssessmentSubmissionDto submission)
        {
            if (submission == null || submission.EmployeeResponses == null || !submission.EmployeeResponses.Any())
            {
                return BadRequest("Invalid submission data.");
            }

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var employeeId = submission.EmployeeId;
                var assessmentId = submission.AssessmentId;

                // 🔹 Remove previous records for the same EmployeeId and AssessmentId
                var existingResponses = await _context.EmployeeResponses
                    .Where(e => e.EmployeeID == employeeId && e.AssessmentID == assessmentId)
                    .ToListAsync();

                if (existingResponses.Any())
                {
                    _context.EmployeeResponses.RemoveRange(existingResponses);
                    await _context.SaveChangesAsync();  // Commit deletion first
                }

                // 🔹 Store new responses in EmployeeResponse table
                foreach (var response in submission.EmployeeResponses)
                {
                    var employeeResponse = new EmployeeResponse
                    {
                        EmployeeID = employeeId,
                        AssessmentID = assessmentId,
                        QuestionID = response.QuestionID,
                        SelectedOption = response.SelectedOption
                    };

                    _context.EmployeeResponses.Add(employeeResponse);
                }

                await _context.SaveChangesAsync();

                // ✅ Retrieve correct answers from Question table
                var questionIds = submission.EmployeeResponses.Select(r => r.QuestionID).ToList();
                var correctAnswers = await _context.Questions
                    .Where(q => questionIds.Contains(q.QuestionID))
                    .ToDictionaryAsync(q => q.QuestionID, q => q.CorrectAns);

                // ✅ Get number of questions conducted from the assessment (not total)
                int questionConduct = await _context.Assessments
                    .Where(a => a.AssessmentID == assessmentId)
                    .Select(a => a.QuestionConduct)
                    .FirstOrDefaultAsync();

                if (questionConduct <= 0)
                {
                    return BadRequest("Invalid QuestionConduct value in the assessment.");
                }

              


                // ✅ Calculate score
                int score = submission.EmployeeResponses.Count(r =>
                    correctAnswers.ContainsKey(r.QuestionID) && correctAnswers[r.QuestionID] == r.SelectedOption
                );
                int percentage = (score * 100) / questionConduct;

                int passPercentage = _context.Assessments
                    .Where(e => e.AssessmentID == assessmentId)
                    .Select(e => e.PassPercentage)
                    .FirstOrDefault();

                // ✅ Update or Insert into AssessmentScore table with AttemptCount
                var assessmentScore = await _context.AssessmentScores
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AssessmentID == assessmentId);

                if (assessmentScore != null)
                {
                    // Update existing record
                    assessmentScore.Score = score;
                    assessmentScore.DateTaken = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"));
                    assessmentScore.IsTaken = true;
                    assessmentScore.Status = percentage >= passPercentage;
                    assessmentScore.AttemptCount += 1; // Increment attempt count
                    _context.AssessmentScores.Update(assessmentScore);
                }
                else
                {
                    // Create new record
                    assessmentScore = new AssessmentScore
                    {
                        EmployeeId = employeeId,
                        AssessmentID = assessmentId,
                        Score = score,
                        IsTaken = true,
                        Status = percentage >= passPercentage,
                        AttemptCount = 1, // First attempt
                        DateTaken = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
                            TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
                    };
                    _context.AssessmentScores.Add(assessmentScore);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return Ok(new
                {
                    Message = "Exam submitted successfully",
                    Score = score,
                    TotalQuestions = questionConduct,
                   
                    AttemptCount = assessmentScore.AttemptCount // Optional: return attempt count
                });
              
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred: {ex.Message}");
            }
        }




        [HttpGet("employee-responses/{employeeId}/{assessmentId}")]
        public async Task<IActionResult> GetEmployeeResponses(int employeeId, int assessmentId)
        {
            try
            {
                var assessmentScore = await _context.AssessmentScores
                    .FirstOrDefaultAsync(a => a.EmployeeId == employeeId && a.AssessmentID == assessmentId);

                if (assessmentScore == null || !assessmentScore.IsTaken)
                {
                    return NotFound(new { message = "No completed assessment found for this employee and assessment ID" });
                }

                var responses = await _context.EmployeeResponses
                    .Where(r => r.EmployeeID == employeeId && r.AssessmentID == assessmentId)
                    .Join(_context.Questions,
                        response => response.QuestionID,
                        question => question.QuestionID,
                        (response, question) => new
                        {
                            QuestionId = question.QuestionID,
                            QuestionText = question.QuestionText,
                            EmployeeAnswer = response.SelectedOption,
                            CorrectAnswer = question.CorrectAns,
                            IsCorrect = response.SelectedOption == question.CorrectAns
                        })
                    .ToListAsync();

                var assessment = await _context.Assessments
                    .Where(a => a.AssessmentID == assessmentId)
                    .Select(a => new
                    {
                        Topic = a.Topic,
                        Description = a.Description,
                        TotalQuestions = a.TotalQuestions,
                        QuestionConduct=a.QuestionConduct,
                        PassingPercentage = a.PassPercentage
                    })
                    .FirstOrDefaultAsync();

                if (responses == null || !responses.Any())
                {
                    return NotFound(new { message = "No responses found for this assessment" });
                }

                int correctCount = responses.Count(r => r.IsCorrect);

                return Ok(new
                {
                    AssessmentDetails = new
                    {
                        AssessmentId = assessmentId,
                        assessment.Topic,
                        assessment.Description,
                        assessment.TotalQuestions,
                        assessment.PassingPercentage,
                        Score = assessmentScore.Score,
                        DateTaken = assessmentScore.DateTaken,
                        assessment.QuestionConduct,
                        Percentage = (correctCount * 100) / assessment.QuestionConduct,
                     
                    },
                    Responses = responses.Select(r => new
                    {
                        r.QuestionId,
                        r.QuestionText,
                        r.EmployeeAnswer,
                        r.CorrectAnswer,
                        r.IsCorrect
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while fetching employee responses",
                    error = ex.Message
                });
            }
        }

        [HttpGet("all-assessments")]

        [Authorize(Roles = "Admin,Manager")]

        public async Task<IActionResult> GetAllAssessments()

        {

            try

            {

                var assessments = await _context.Assessments

                    .Select(a => new

                    {

                        assessmentId = a.AssessmentID,

                        topic = a.Topic,

                        description = a.Description,

                    })

                    .ToListAsync();

                return Ok(assessments);

            }

            catch (Exception ex)

            {

                return StatusCode(500, new { message = "An error occurred while fetching all assessments", error = ex.Message });

            }

        }



        [HttpGet("EditAssessment/{ExamId}")]
        public async Task<IActionResult> GetQuestionsToEdit(int ExamId)
        {
            var questions = await _context.Questions
                .Where(e => e.ExamID == ExamId)
                .Select(q => new
                {
                    q.QuestionID,
                    q.ExamID,
                    q.QuestionText,
                    q.OptionA,
                    q.OptionB,
                    q.OptionC,
                    q.OptionD,
                    q.CorrectAns
                })
                .ToListAsync();

            return Ok(questions);
        }

        [HttpPost("updateQuestions")]
        public async Task<IActionResult> UpdateQuestions([FromBody] List<UpdateQuestionDto> updatedQuestions)
        {
            if (updatedQuestions == null || !updatedQuestions.Any())
                return BadRequest("No questions provided for update.");

            int examId = updatedQuestions.First().ExamID;

            // 🔄 Remove existing questions for the exam
            var existingQuestions = await _context.Questions
                .Where(q => q.ExamID == examId)
                .ToListAsync();

            _context.Questions.RemoveRange(existingQuestions);
            await _context.SaveChangesAsync();

            // 💾 Map DTOs to Entities and Add updated questions
            var questionsToAdd = updatedQuestions.Select(dto => new Question
            {
                QuestionID = 0, // Reset for auto-increment
                ExamID = dto.ExamID,
                QuestionText = dto.QuestionText,
                OptionA = dto.OptionA,
                OptionB = dto.OptionB,
                OptionC = dto.OptionC,
                OptionD = dto.OptionD,
                CorrectAns = dto.CorrectAns
            }).ToList();

            _context.Questions.AddRange(questionsToAdd);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Questions updated successfully." });
        }




        [HttpDelete("deleteExam/{assessmentId}")]

        public async Task<IActionResult> DeleteNotTakenAssessment(int assessmentId)

        {

            var assessments = await _context.AssessmentScores

                .Where(score => score.AssessmentID == assessmentId && score.IsTaken == false)

                .ToListAsync();

            if (assessments.Count == 0)

            {

                return NotFound(new { message = "No assessments found that have not been taken" });

            }

            _context.AssessmentScores.RemoveRange(assessments);

            await _context.SaveChangesAsync();

            return Ok(new { message = "Assessments deleted successfully!" });

        }


        [HttpGet("all-scores")]
        [Authorize(Roles = "Manager,Admin")]
        public async Task<IActionResult> GetAllScores()
        {
            try
            {
                var allScores = await _context.AssessmentScores
                    .Where(a => a.IsTaken) // Only completed assessments
                    .Join(_context.Employees,
                        score => score.EmployeeId,
                        employee => employee.EmployeeId,
                        (score, employee) => new { score, employee })
                    .Join(_context.Categories, // Join with Categories table
                        combined => combined.employee.CategoryId, // Foreign key in Employees
                        category => category.CategoryId, // Primary key in Categories
                        (combined, category) => new { combined.score, combined.employee, category })
                    .Join(_context.Assessments,
                        combined => combined.score.AssessmentID,
                        assessment => assessment.AssessmentID,
                        (combined, assessment) => new
                        {
                            userEmail = combined.employee.Email,
                            userName = combined.employee.Username,
                            assignmentName = assessment.Topic,
                            score = combined.score.Score,
                            description = assessment.Description,
                            examId = combined.score.AssessmentID,
                            totalQuestions = assessment.TotalQuestions,
                            questionConduct=assessment.QuestionConduct,
                            dateTaken = combined.score.DateTaken,
                            category = combined.category.CategoryName,// Fetch CategoryName
                            passPercentage = assessment.PassPercentage
                        })
                    .ToListAsync();

                return Ok(allScores);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while fetching all scores", error = ex.Message });
            }
        }

    }
}

