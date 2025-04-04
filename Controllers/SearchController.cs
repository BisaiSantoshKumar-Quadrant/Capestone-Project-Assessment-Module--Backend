using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAssessment_project.Data;
using QAssessment_project.DTO;

namespace QAssessment_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SearchController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public SearchController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("getCategoryEmails{categoryName}")]
        public async Task<ActionResult<List<object>>> GetEmails(string categoryName)
        {
            try
            {
                if (categoryName != "All")
                {
                    int categoryId = await _context.Categories
                        .Where(c => c.CategoryName == categoryName)
                        .Select(r => r.CategoryId)
                        .FirstOrDefaultAsync();

                    int roleId = await _context.Roles
                        .Where(c => c.RoleName == "User")
                        .Select(r => r.RoleID)
                        .FirstOrDefaultAsync();

                    var emailUsernames = await _context.Employees
                        .Where(c => c.CategoryId == categoryId && c.RoleID == roleId)
                        .Select(e => new { e.Email, e.Username })
                        .ToListAsync();

                    return Ok(emailUsernames);
                }
                else
                {
                    int roleId = await _context.Roles
                        .Where(c => c.RoleName == "User")
                        .Select(r => r.RoleID)
                        .FirstOrDefaultAsync();

                    var emailUsernames = await _context.Employees
                        .Where(c => c.RoleID == roleId)
                        .Select(e => new { e.Email, e.Username })
                        .ToListAsync();
                    return Ok(emailUsernames);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving emails");
            }
        }

        [HttpGet("getCategoryAssessments{categoryName}")]
        public async Task<ActionResult<List<object>>> GetAssessments(string categoryName)
        {
            try
            {
                if (categoryName != "All")
                {
                    int categoryId = await _context.Categories
                        .Where(c => c.CategoryName == categoryName)
                        .Select(r => r.CategoryId)
                        .FirstOrDefaultAsync();

                    var examsDescription = await _context.Assessments
                        .Where(c => c.CategoryId == categoryId)
                        .Select(e => new { e.Topic, e.Description })
                        .ToListAsync();

                    return Ok(examsDescription);
                }
                else
                {
                    var examsDescription = await _context.Assessments
                        .Select(e => new { e.Topic, e.Description })
                        .ToListAsync();

                    return Ok(examsDescription);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving assessments");
            }
        }

        [HttpGet("getAssessmentDetailsByEmail/{email}")]
        public IActionResult GetAssessmentDetailsByEmail(string email)
        {
            var employee = _context.Employees.FirstOrDefault(e => e.Email == email);
            if (employee == null)
            {
                return NotFound("Employee not found.");
            }

            var assessmentScores = _context.AssessmentScores
                .Where(a => a.EmployeeId == employee.EmployeeId)
                .ToList();

            var assessmentDetails = assessmentScores
                .Where(e => e.IsTaken == true)
                .Select(score => new
                {
                    score.AssessmentID,
                    score.EmployeeId,
                    score.Score,
                    score.DateTaken,
                    score.IsTaken,
                    score.Status,
                    Assessment = _context.Assessments.FirstOrDefault(a => a.AssessmentID == score.AssessmentID)
                })
                .Select(ad => new
                {
                    ad.AssessmentID,
                    ad.EmployeeId,
                    ad.Score,
                    ad.DateTaken,
                    ad.IsTaken,
                    ad.Status,
                    ad.Assessment.Topic,
                    ad.Assessment.Description,
                    ad.Assessment.TotalQuestions
                })
                .ToList();

            return Ok(assessmentDetails);
        }

        [HttpGet("getAssessmentTakenUser")]
        public IActionResult GetAssessmentTakenUser([FromQuery] TakenAssesmentDTO tadto)
        {
            var assessment = _context.Assessments
                .FirstOrDefault(e => e.Topic == tadto.Topic && e.Description == tadto.Description);

            if (assessment == null)
            {
                return NotFound("Assessment not found.");
            }

            var userAssessments = _context.AssessmentScores
                .Where(e => e.AssessmentID == assessment.AssessmentID)
                .ToList();

            var assessmentDetails = userAssessments
                .Select(score => new
                {
                    score.AssessmentID,
                    EmployeeEmail = _context.Employees.FirstOrDefault(e => e.EmployeeId == score.EmployeeId)?.Email ?? "Unknown",
                    score.Score,
                    score.DateTaken,
                    score.IsTaken,
                    score.Status,
                    Assessment = _context.Assessments.FirstOrDefault(a => a.AssessmentID == score.AssessmentID)
                })
                .Where(e => e.IsTaken == true)
                .Select(ad => new
                {
                    ad.AssessmentID,
                    ad.EmployeeEmail,
                    ad.Score,
                    ad.DateTaken,
                    ad.IsTaken,
                    ad.Status,
                    ad.Assessment.Topic,
                    ad.Assessment.Description,
                    ad.Assessment.TotalQuestions
                })
                .ToList();

            return Ok(assessmentDetails);
        }

        [HttpGet("getCategoryAssessmentsAll")]
        public async Task<ActionResult<List<object>>> GetAllAssessments()
        {
            try
            {
                var allAssessments = await _context.AssessmentScores
                    .Include(a => a.Assessment)
                    .Include(a => a.Employee)
                    .Where(e => e.IsTaken == true)
                    .Select(score => new
                    {
                        employeeEmail = score.Employee.Email,
                        topic = score.Assessment.Topic,
                        description = score.Assessment.Description,
                        score = score.Score,
                        dateTaken = score.DateTaken,
                        isTaken = score.IsTaken,
                        status = score.Status,
                        categoryName = _context.Categories
                            .Where(c => c.CategoryId == score.Assessment.CategoryId)
                            .Select(c => c.CategoryName)
                            .FirstOrDefault()
                    })
                    .ToListAsync();

                return Ok(allAssessments);
            }
            catch (Exception ex)
            {
                return StatusCode(500, "An error occurred while retrieving assessments");
            }
        }
    }
}