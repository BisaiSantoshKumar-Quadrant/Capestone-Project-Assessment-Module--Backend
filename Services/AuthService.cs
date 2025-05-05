using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using QAssessment_project.Data;
using QAssessment_project.Model;
using QAssessment_project.DTO;
using BC = BCrypt.Net.BCrypt;

namespace QAssessment_project.Services
{
    public class AuthService : IAuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;
        private readonly IOTPService _otpService;

        public AuthService(ApplicationDbContext context, IConfiguration configuration, IEmailService emailService, IOTPService otpService)
        {
            _context = context;
            _configuration = configuration;
            _emailService = emailService;
            _otpService = otpService;
        }

        public async Task<bool> SendOTPForRegistrationAsync(string email)
        {
            if (await _context.Employees.AnyAsync(u => u.Email == email))
                return false; // Email already exists

            await _otpService.SendOTPAsync(email);
            return true;
        }

        public async Task<bool> VerifyOTPForRegistrationAsync(string email, string otp)
        {
            return await _otpService.VerifyOTPAsync(email, otp);
        }

        public async Task<string> RegisterAsync(RegisterDTO model)
        {
            try
            {
                // Check if email is already registered
                if (await _context.Employees.AnyAsync(u => u.Email == model.Email))
                    return "User already exists!";

                // Fetch or create the User role
                string roleName = "User";
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
                if (role == null)
                {
                    role = new Role { RoleName = "User" };
                    _context.Roles.Add(role);
                    await _context.SaveChangesAsync();
                }

                // Fetch the category
                var category = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == model.CategoryName);
                if (category == null)
                {
                    return $"Category '{model.CategoryName}' not found. Please seed category first.";
                }

                // Create new employee record after OTP verification
                var employee = new Employee
                {
                    Email = model.Email,
                    Username = model.Username,
                    Password = BC.HashPassword(model.Password),
                    RoleID = role.RoleID,
                    CategoryId = category.CategoryId,
                    JoinedDate = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
                    )
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                // Fetch assessments for this category
                var assessments = await _context.Assessments
                    .Where(a => a.CategoryId == category.CategoryId)
                    .ToListAsync();

                // Insert assessment scores for the user
                var assessmentScores = assessments.Select(a => new AssessmentScore
                {
                    EmployeeId = employee.EmployeeId,
                    AssessmentID = a.AssessmentID,
                    IsTaken = false,
                    CategoryId = category.CategoryId
                }).ToList();

                _context.AssessmentScores.AddRange(assessmentScores);
                await _context.SaveChangesAsync();

                return "User registered successfully!";
            }
            catch (Exception ex)
            {
                return $"Registration failed: {ex.Message}";
            }
        }

        public async Task<string> ManagerRegisterAsync(ManagerRegisterDTO model)
        {
            try
            {
                // Check if email is already registered
                if (await _context.Employees.AnyAsync(u => u.Email == model.Email))
                    return "User already exists!";

                // Verify secret key
                const string secretKey = "Admin@1199";
                if (model.SecretKey != secretKey)
                    return "Invalid secret key!";

                // Fetch or create the Manager role
                var managerRole = await _context.Roles.FirstOrDefaultAsync(r => r.RoleName == "Admin");
                if (managerRole == null)
                {
                    managerRole = new Role { RoleName = "Admin" };
                    _context.Roles.Add(managerRole);
                    await _context.SaveChangesAsync();
                }

                // Fetch or create the Management category
                var managerCategory = await _context.Categories.FirstOrDefaultAsync(c => c.CategoryName == "Management");
                if (managerCategory == null)
                {
                    managerCategory = new Category { CategoryName = "Management" };
                    _context.Categories.Add(managerCategory);
                    await _context.SaveChangesAsync();
                }

                // Ensure only one Manager is allowed
                if (await _context.Employees.AnyAsync(e => e.RoleID == managerRole.RoleID))
                    return "A Admin already exists! Only one Manager is allowed.";

                // Create new employee record after OTP verification
                var employee = new Employee
                {
                    Email = model.Email,
                    Username = model.Username,
                    Password = BC.HashPassword(model.Password),
                    RoleID = managerRole.RoleID,
                    CategoryId = managerCategory.CategoryId,
                    JoinedDate = TimeZoneInfo.ConvertTimeFromUtc(
                        DateTime.UtcNow,
                        TimeZoneInfo.FindSystemTimeZoneById("India Standard Time")
                    )
                };

                _context.Employees.Add(employee);
                await _context.SaveChangesAsync();

                return "Admin registered successfully!";
            }
            catch (Exception ex)
            {
                return $"Registration failed: {ex.Message}";
            }
        }

        public async Task<LoginResultDTO> LoginAsync(LoginDTO model)
        {
            var user = await _context.Employees.Include(e => e.Role)
                .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
                return new LoginResultDTO { Token = "USER_NOT_FOUND" };

            if (!BC.Verify(model.Password, user.Password))
                return new LoginResultDTO { Token = "INVALID_PASSWORD" };

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
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == email);
            if (user == null)
            {
                return (true, "If your email is registered, you will receive a password reset link");
            }

            var token = Guid.NewGuid().ToString();
            user.ResetToken = token;
            user.ResetTimeExpires = DateTime.UtcNow.AddHours(24);

            await _context.SaveChangesAsync();

            var resetUrl = $"https://purple-bush-095dd6c1e.6.azurestaticapps.net/reset-password?email={email}&token={token}";
            var emailBody = $"Please reset your password by clicking <a href='{resetUrl}'>here</a>. The link is valid for 24 hours.";

            await _emailService.SendEmailAsync(email, "Reset Your Password", emailBody);

            return (true, "If your email is registered, you will receive a password reset link");
        }

        public async Task<(bool Success, string Message)> ResetPasswordAsync(ResetPasswordRequestDTO request)
        {
            var user = await _context.Employees.FirstOrDefaultAsync(u => u.Email == request.Email);
            if (user == null || user.ResetToken != request.Token || user.ResetTimeExpires < DateTime.UtcNow)
            {
                return (false, "Invalid or expired token");
            }

            user.Password = BC.HashPassword(request.NewPassword);
            user.ResetToken = null;
            user.ResetTimeExpires = null;

            await _context.SaveChangesAsync();

            return (true, "Password has been reset successfully");
        }
    }
}
