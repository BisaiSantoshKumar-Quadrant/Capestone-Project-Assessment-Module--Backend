
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Threading.Tasks;
using QAssessment_project.Data;
using Microsoft.EntityFrameworkCore;
 
namespace QAssessment_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ActiveUsersController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ActiveUsersController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Endpoint 1: Fetch users with RoleID = 4
        [HttpGet("active")]
        public async Task<IActionResult> GetActiveUsers()
        {
            try
            {
                // Fetch employees with RoleID = 4
                var activeUsers = await _context.Employees
                    .Where(e => e.RoleID == 4)
                    .Select(e => new { e.Username, e.Email, e.RoleID }) // Select specific fields
                    .ToListAsync();

                return Ok(activeUsers); // Return as JSON
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                return StatusCode(500, new { message = "An error occurred while fetching active users.", error = ex.Message });
            }
        }

        [HttpPost("updaterole/{email}")]
        public async Task<IActionResult> UpdateEmployeeRole([FromRoute] string email)
        {
            // Validate the request
            if (string.IsNullOrEmpty(email))
            {
                return BadRequest(new { message = "Email is required." });
            }

            try
            {
                // Find the employee by email
                var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
                if (employee == null)
                {
                    return NotFound(new { message = "Employee not found." });
                }

                // Check if the current RoleID is 4
                if (employee.RoleID != 4)
                {
                    return BadRequest(new { message = "Employee RoleID is not 4 and cannot be updated." });
                }

                // Update RoleID to 1
                employee.RoleID = 1;
                await _context.SaveChangesAsync();

                return Ok(new { message = "Employee RoleID updated successfully." });
            }
            catch (Exception ex)
            {
                // Handle errors gracefully
                return StatusCode(500, new { message = "An error occurred while updating the role.", error = ex.Message });
            }
        }

         
    }
}
