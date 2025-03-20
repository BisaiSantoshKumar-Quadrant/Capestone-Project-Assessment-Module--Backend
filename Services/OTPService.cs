namespace QAssessment_project.Services
{
    public class OTPService: IOTPService
    {
        public void SendOTPToEmployee(string employeeEmail)
        {
            string otp = OTPGenerator.GenerateOTP(6); // Generate a 6-digit OTP
            EmailSender.SendEmail(employeeEmail, otp); // Send the OTP to the employee's email
        }
    }
}
