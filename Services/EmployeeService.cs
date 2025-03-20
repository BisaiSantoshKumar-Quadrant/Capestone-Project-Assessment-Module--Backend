using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QAssessment_project.Data;
using QAssessment_project.Model;
namespace QAssessment_project.Services
{
  

    public class EmployeeService : IEmployeeService
    {
        private readonly ApplicationDbContext _context;

        public EmployeeService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Employee>> GetEmployeesAsync()
        {
            return await _context.Employees.ToListAsync();
        }
    }

    public interface IEmployeeService
    {
        Task<List<Employee>> GetEmployeesAsync();
    }
}
