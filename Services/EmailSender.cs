namespace QAssessment_project.Services
{
    public class EmailSender
    {
        public static void SendEmail(string toEmail, string otp)
        {
            var fromAddress = new System.Net.Mail.MailAddress("rajubutterfly1199@gmail.com", "KapaRajashekarReddy");
            var toAddress = new System.Net.Mail.MailAddress(toEmail);
            const string fromPassword = "KReddy1199**";
            const string subject = "Your OTP Code";
            string body = $"Your OTP code is: {otp}";

            var smtp = new System.Net.Mail.SmtpClient
            {
                Host = "smtp.example.com",
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = System.Net.Mail.SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential(fromAddress.Address, fromPassword)
            };

            using (var message = new System.Net.Mail.MailMessage(fromAddress, toAddress)
            {
                Subject = subject,
                Body = body
            })
            {
                smtp.Send(message);
            }
        }
    }
}
