using Microsoft.EntityFrameworkCore;
using QAssessment_project.Data;
using QAssessment_project.Model;

namespace QAssessment_project.Services
{
    public class OTPService : IOTPService
    {
        private readonly ApplicationDbContext _context;
        private readonly IEmailService _emailService;

        public OTPService(ApplicationDbContext context, IEmailService emailService)
        {
            _context = context;
            _emailService = emailService;
        }

        public string GenerateOTP(int length)
        {
            var random = new Random();
            var chars = "0123456789";
            return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }

        public async Task SendOTPAsync(string email)
        {
            var otp = GenerateOTP(6);
            var otpRecord = await _context.OTPRecords.FirstOrDefaultAsync(r => r.Email == email);
            if (otpRecord == null)
            {
                otpRecord = new OTPRecord
                {
                    Email = email,
                    OTP = otp,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(10)
                };
                _context.OTPRecords.Add(otpRecord);
            }
            else
            {
                otpRecord.OTP = otp;
                otpRecord.ExpiresAt = DateTime.UtcNow.AddMinutes(10);
            }
            await _context.SaveChangesAsync();

            var emailBody = $"Your OTP for email verification is: {otp}. It expires in 10 minutes.";
            await _emailService.SendEmailAsync(email, "Verify Your Email", emailBody);
        }

        public async Task<bool> VerifyOTPAsync(string email, string otp)
        {
            var otpRecord = await _context.OTPRecords.FirstOrDefaultAsync(r => r.Email == email);
            if (otpRecord == null || otpRecord.OTP != otp || otpRecord.ExpiresAt < DateTime.UtcNow)
            {
                return false;
            }
            // Optionally, remove the OTP record after successful verification
            _context.OTPRecords.Remove(otpRecord);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}