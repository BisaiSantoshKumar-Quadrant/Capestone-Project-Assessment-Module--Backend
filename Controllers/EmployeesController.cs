using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QAssessment_project.Data;
using QAssessment_project.DTO;
using QAssessment_project.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace QAssessment_project.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EmployeesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/employees
        [HttpGet]
        [Authorize] // Require authentication
        public async Task<ActionResult<IEnumerable<EmployeeRoleDTO>>> GetEmployees()
        {
            // Get the logged-in user's email from the JWT token
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrEmpty(currentUserEmail))
            {
                return Unauthorized(new { message = "Unable to identify the current user." });
            }

            var employees = await _context.Employees
                .Include(e => e.Role)
                .Where(e => e.Email != currentUserEmail) // Exclude the current user
                .Select(e => new EmployeeRoleDTO
                {
                    Email = e.Email,
                    RoleName = e.Role.RoleName,
                    Username = e.Username
                })
                .ToListAsync();

            return Ok(employees);
        }

        // GET: api/employees/roles
        [HttpGet("roles")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RoleNameDTO>>> GetRoles()
        {
            var roleNames = await _context.Roles
                .Select(r => new RoleNameDTO { RoleName = r.RoleName })
                .Distinct()
                .ToListAsync();
            return Ok(roleNames);
        }

        // PUT: api/employees/submit
        [HttpPut("submit")]
        [Authorize(Roles = "Manager")]
        public async Task<IActionResult> UpdateEmployeeRoleByEmail([FromBody] UpdateRoleByEmailDTO updateRoleByEmailDTO)
        {
            // Optional: Keep this check as a fallback, though it’s unlikely to be hit due to frontend filtering
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (currentUserEmail == updateRoleByEmailDTO.Email)
            {
                return BadRequest(new { message = "You cannot change your own role." });
            }

            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == updateRoleByEmailDTO.RoleName);
            if (role == null)
            {
                return NotFound(new { message = "Role not found" });
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == updateRoleByEmailDTO.Email);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            employee.RoleID = role.RoleID;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{email}")]
        [Authorize(Roles = "Admin")] // Restrict to Admin only
        public async Task<IActionResult> DeleteEmployee(string email)
        {
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (currentUserEmail == email)
            {
                return BadRequest(new { message = "You cannot delete yourself." });
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            _context.Employees.Remove(employee);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Employee deleted successfully" });
        }

    }
}