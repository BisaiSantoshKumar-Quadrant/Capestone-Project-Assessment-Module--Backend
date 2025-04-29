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

        [HttpGet("getCategories")]
        public async Task<ActionResult<List<string>>> GetCategories()
        {
            try
            {
                List<string> categories = await _context.Categories
                    .Select(r => r.CategoryName)
                    .ToListAsync();

                return Ok(categories);
            }
            catch (Exception ex)
            {
                // Consider logging the exception here
                return StatusCode(500, "An error occurred while retrieving categories");
            }
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
                    Username = e.Username,
                    Category = e.Category != null ? e.Category.CategoryName : null,
                    EmployeeId = e.EmployeeId
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
        [Authorize(Roles = "Manager,Admin")]
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

        [HttpDelete("DeleteEmployee/{email}")]
        [Authorize(Roles = "Admin")] // Restrict to Admin only
        public async Task<IActionResult> DeleteEmployee(string email)
        {
            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == email);
            if (employee != null)
            {
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();
                return Ok(); // Optional: return success response
            }
            else
            {
                return NotFound(); // Optional: return 404 if the employee is not found
            }
        }

        [HttpPost("addRole/{dto}")]
        public async Task<IActionResult> AddRole(string dto)
        {
            Console.WriteLine(dto);
            try
            {
                // Validate input
                if (dto == null || string.IsNullOrEmpty(dto))
                {
                    return BadRequest("Role name is required");
                }

                // Check if role already exists
                if (await _context.Roles.AnyAsync(r => r.RoleName == dto))
                {
                    return Conflict("Role already exists");
                }

                var roleEntity = new Role
                {
                    RoleName = dto
                };
                _context.Roles.Add(roleEntity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Role added successfully" });
            }
            catch (Exception ex)
            {
                // Consider logging the exception in production
                return StatusCode(500, $"Failed to add role: {ex.Message}");
            }
        }

        [HttpPost("addCategory/{category}")]
        public async Task<IActionResult> AddCategory(string category)
        {
            try
            {
                Console.WriteLine(category);
                if (string.IsNullOrEmpty(category))
                {
                    return BadRequest("Category name is required");
                }

                // Check if category already exists
                if (await _context.Categories.AnyAsync(c => c.CategoryName == category))
                {
                    return Conflict("Category already exists");
                }

                var categoryEntity = new Category
                {
                    CategoryName = category
                };
                _context.Categories.Add(categoryEntity);
                await _context.SaveChangesAsync();

                return Ok(new { Message = "Category added successfully", CategoryId = categoryEntity.CategoryId });
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                return StatusCode(500, $"Failed to add category: {ex.Message}");
            }
        }

        [HttpGet("IsManagerPresent")]
        public async Task<ActionResult<bool>> IsManagerPresent()
        {
            // Check if there is any employee with the role "Manager"
            bool managerExists = await _context.Employees
                .Include(e => e.Role)
                .AnyAsync(e => e.Role.RoleName == "Admin");

            return Ok(managerExists);
        }

        [HttpPut("update-category")]
        [Authorize(Roles = "Manager, Admin")] // Restrict to Manager and Admin
        public async Task<IActionResult> UpdateEmployeeCategoryByEmail([FromBody] UpdateCategoryByEmailDTO updateCategoryByEmailDTO)
        {
            var currentUserEmail = User.FindFirst(ClaimTypes.Email)?.Value;
            if (currentUserEmail == updateCategoryByEmailDTO.Email)
            {
                return BadRequest(new { message = "You cannot change your own category." });
            }

            var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == updateCategoryByEmailDTO.CategoryName);
            if (category == null)
            {
                return NotFound(new { message = "Category not found" });
            }

            var employee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == updateCategoryByEmailDTO.Email);
            if (employee == null)
            {
                return NotFound(new { message = "Employee not found" });
            }

            employee.CategoryId = category.CategoryId;
            await _context.SaveChangesAsync();

            return NoContent();
        }


        // PUT: api/employees/updateCategory
        [HttpPut("updateCategory")]
        [Authorize(Roles = "Manager,Admin")] // Restrict to Manager and Admin roles
        public async Task<IActionResult> UpdateCategory([FromBody] UpdateCategoryNameDTO updateCategoryNameDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(updateCategoryNameDTO.OldCategoryName) || string.IsNullOrEmpty(updateCategoryNameDTO.NewCategoryName))
                {
                    return BadRequest(new { message = "Both old and new category names are required" });
                }

                if (updateCategoryNameDTO.OldCategoryName == updateCategoryNameDTO.NewCategoryName)
                {
                    return BadRequest(new { message = "New category name must be different from the old name" });
                }

                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == updateCategoryNameDTO.OldCategoryName);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                // Check if the new category name already exists
                if (await _context.Categories.AnyAsync(c => c.CategoryName == updateCategoryNameDTO.NewCategoryName))
                {
                    return Conflict(new { message = "A category with this name already exists" });
                }

                category.CategoryName = updateCategoryNameDTO.NewCategoryName;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update category: {ex.Message}");
            }
        }

        // PUT: api/employees/updateRole
        [HttpPut("updateRole")]
        [Authorize(Roles = "Manager,Admin")] // Restrict to Manager and Admin roles
        public async Task<IActionResult> UpdateRole([FromBody] UpdateRoleNameDTO updateRoleNameDTO)
        {
            try
            {
                if (string.IsNullOrEmpty(updateRoleNameDTO.OldRoleName) || string.IsNullOrEmpty(updateRoleNameDTO.NewRoleName))
                {
                    return BadRequest(new { message = "Both old and new role names are required" });
                }

                if (updateRoleNameDTO.OldRoleName == updateRoleNameDTO.NewRoleName)
                {
                    return BadRequest(new { message = "New role name must be different from the old name" });
                }

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == updateRoleNameDTO.OldRoleName);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // Check if the new role name already exists
                if (await _context.Roles.AnyAsync(r => r.RoleName == updateRoleNameDTO.NewRoleName))
                {
                    return Conflict(new { message = "A role with this name already exists" });
                }

                role.RoleName = updateRoleNameDTO.NewRoleName;
                await _context.SaveChangesAsync();

                return NoContent();
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Failed to update role: {ex.Message}");
            }
        }


        // DELETE: api/employees/deleteCategory/{categoryName}
        [HttpDelete("deleteCategory/{categoryName}")]
        [Authorize(Roles = "Manager,Admin")] // Restrict to Manager role
        public async Task<IActionResult> DeleteCategory(string categoryName)
        {
            try
            {
                if (string.IsNullOrEmpty(categoryName))
                {
                    return BadRequest(new { message = "Category name is required" });
                }

                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == categoryName);
                if (category == null)
                {
                    return NotFound(new { message = "Category not found" });
                }

                // Check if any employees are assigned to this category
                var employeesInCategory = await _context.Employees.AnyAsync(e => e.CategoryId == category.CategoryId);
                if (employeesInCategory)
                {
                    return BadRequest(new { message = "Cannot delete category because it is assigned to one or more employees" });
                }

                _context.Categories.Remove(category);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Category deleted successfully" });
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                return StatusCode(500, $"Failed to delete category: {ex.Message}");
            }
        }




        // DELETE: api/employees/deleteRole/{roleName}
        [HttpDelete("deleteRole/{roleName}")]
        [Authorize(Roles = "Manager,Admin")] // Restrict to Manager role
        public async Task<IActionResult> DeleteRole(string roleName)
        {
            try
            {
                if (string.IsNullOrEmpty(roleName))
                {
                    return BadRequest(new { message = "Role name is required" });
                }

                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                {
                    return NotFound(new { message = "Role not found" });
                }

                // Check if any employees are assigned to this role
                var employeesInRole = await _context.Employees.AnyAsync(e => e.RoleID == role.RoleID);
                if (employeesInRole)
                {
                    return BadRequest(new { message = "Cannot delete role because it is assigned to one or more employees" });
                }

                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Role deleted successfully" });
            }
            catch (Exception ex)
            {
                // Consider logging the exception
                return StatusCode(500, $"Failed to delete role: {ex.Message}");
            }
        }
    }
}