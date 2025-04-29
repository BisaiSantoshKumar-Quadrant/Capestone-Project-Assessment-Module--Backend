namespace QAssessment_project.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using QAssessment_project.DTO;
    using QAssessment_project.Services;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Cors;
    using Microsoft.AspNetCore.Authorization;

    [Route("api/auth")]
    [ApiController]
    [EnableCors("AllowSpecificOrigins")] // Apply CORS policy to this controller
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IOTPService _otpService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
            //_otpService = otpService;
        }

        [HttpPost("mangerregister")]
        public async Task<IActionResult> ManagerRegister([FromBody] ManagerRegisterDTO model)
        {
            var result = await _authService.ManagerRegisterAsync(model);

            if (result == "Admin already exists!")
                return Conflict(new { message = result });

            if (result == "Default role 'User' not found. Please seed roles first.")
                return BadRequest(new { message = result });

            // Success case
            return Ok(new { message = result });
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            var result = await _authService.RegisterAsync(model);

            if (result == "User already exists!")
                return Conflict(new { message = result });

            if (result == "Default role 'User' not found. Please seed roles first.")
                return BadRequest(new { message = result });

            // Success case
            return Ok(new { message = result });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            var loginResult = await _authService.LoginAsync(model);

            if (loginResult.Token == "USER_NOT_FOUND")
                return Unauthorized(new { message = "User not registered. Please register first.", errorType = "unregistered" });

            if (loginResult.Token == "INVALID_PASSWORD")
                return Unauthorized(new { message = "Invalid credentials. Please enter correct password.", errorType = "invalid_credentials" });

            return Ok(new { token = loginResult.Token, role = loginResult.Role, employeeId = loginResult.EmployeeId, name = loginResult.Username, email = loginResult.Email });
        }

        /*[HttpPost("send-otp")]
        public IActionResult SendOTP([FromBody] string employeeEmail)
        {
            if (string.IsNullOrEmpty(employeeEmail))
            {
                return BadRequest("Employee email is required.");
            }

            _otpService.SendOTPToEmployee(employeeEmail);
            return Ok("OTP sent successfully.");
        }*/

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDTO request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request.Email);
                return Ok(new { message = result.Message });
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"Error in forgot password: {ex.Message}");
                return StatusCode(500, new { message = "An error occurred processing your request." });
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequestDTO request)
        {
            var result = await _authService.ResetPasswordAsync(request);

            if (!result.Success)
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        [HttpPost("send-otp")]
        public async Task<IActionResult> SendOTP([FromBody] string email)
        {
            var result = await _authService.SendOTPForRegistrationAsync(email);
            if (!result)
                return Conflict("Email already exists!");
            return Ok("OTP sent to your email.");
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOTP([FromBody] VerifyOTPDTO model)
        {
            var result = await _authService.VerifyOTPForRegistrationAsync(model.Email, model.OTP);
            return result ? Ok("Email verified!") : BadRequest("Invalid or expired OTP.");

        }

    }
}