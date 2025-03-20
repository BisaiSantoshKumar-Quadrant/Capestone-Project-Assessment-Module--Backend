using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QAssessment_project.Data;
using QAssessment_project.Model;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;
using QAssessment_project.DTO;
using Microsoft.AspNetCore.Mvc;

namespace QAssessment_project.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration,IEmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<string> RegisterAsync(RegisterDTO model)
        {
            if (await _context.Employees.AnyAsync(u => u.Email == model.Email))
                return "User already exists!";

            var defaultRoleName = "Guest";

            // Fetch role, or create it if not exists
            var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == defaultRoleName);
            if (role == null)
                return "Default role 'Guest' not found. Please seed roles first.";

            var employee = new Employee
            {
                Email = model.Email,
                Username = model.Username,
                Password = BC.HashPassword(model.Password), // Secure Password
                RoleID = role.RoleID,
                JoinedDate = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow,
    TimeZoneInfo.FindSystemTimeZoneById("India Standard Time"))
            };

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
            // ✅ Fetch the saved employee to ensure EmployeeId is set
            var savedEmployee = await _context.Employees.FirstOrDefaultAsync(e => e.Email == model.Email);
            if (savedEmployee == null)
                return "Error retrieving saved employee!";

            // ✅ Now we can assign assessments because EmployeeId exists
            var assessments = await _context.Assessments.ToListAsync();
            var assessmentScores = assessments.Select(a => new AssessmentScore
            {
                EmployeeId = savedEmployee.EmployeeId, // Now EmployeeId is confirmed
                AssessmentID = a.AssessmentID,
                Score = 0, // Default score
                IsTaken = false // Mark as not taken
            }).ToList();

            _context.AssessmentScores.AddRange(assessmentScores);
            await _context.SaveChangesAsync(); // Save assessment scores
            return "User registered successfully!";
        }

        public async Task<LoginResultDTO> LoginAsync(LoginDTO model)
        {
            var user = await _context.Employees.Include(e => e.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return new LoginResultDTO { Token = "USER_NOT_FOUND" };

            if (!BC.Verify(model.Password, user.Password))
                return new LoginResultDTO { Token = "INVALID_PASSWORD" };

            // Rest of your token generation code remains unchanged
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_configuration["Jwt:Key"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
            new Claim(ClaimTypes.NameIdentifier, user.EmployeeId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.RoleName)
        }),
                Expires = DateTime.UtcNow.AddHours(3),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            return new LoginResultDTO
            {
                Token = tokenString,
                Role = user.Role.RoleName,
                EmployeeId = user.EmployeeId,
                Username = user.Username,
                Email = user.Email
            };
        }

        public async Task<(bool Success, string Message)> ForgotPasswordAsync(string email)
        {
            // Find the user by email
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == email);

            // Don't reveal that the user doesn't exist
            if (user == null)
            {
                return (true, "If your email is registered, you will receive a password reset link");
            }

            // Generate a random token
            var token = Guid.NewGuid().ToString();

            // Update user with reset token
            user.ResetToken = token;
            user.ResetTimeExpires = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            // Send password reset email
            var resetUrl = $"http://localhost:3000/reset-password?email={email}&token={token}";
            var emailBody = $"Please reset your password by clicking <a href='{resetUrl}'>here</a>. The link is valid for 24 hours.";

            await _emailService.SendEmailAsync(email, "Reset Your Password", emailBody);

            return (true, "If your email is registered, you will receive a password reset link");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            // Find the user by email
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == request.Email);

            // Check if user exists, token is valid, and not expired
            if (user == null || user.ResetToken != request.Token || user.ResetTimeExpires < DateTime.UtcNow)
            {
                return (false, "Invalid or expired token");
            }

            // Update password
            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTimeExpires = null;

            await _context.SaveChangesAsync();

            return (true, "Password has been reset successfully");
        }



        public void SendOTPToEmployee(string employeeEmail)
        {
            string otp = OTPGenerator.GenerateOTP(6); // Generate a 6-digit OTP
            EmailSender.SendEmail(employeeEmail, otp); // Send the OTP to the employee's email
        }
    }
}
