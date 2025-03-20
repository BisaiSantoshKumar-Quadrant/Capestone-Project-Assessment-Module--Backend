namespace QAssessment_project.Services
{
    public class OTPGenerator
    {
        public static string GenerateOTP(int length)
        {
            const string validChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                var otp = new char[length];
                var buffer = new byte[length];

                rng.GetBytes(buffer);

                for (int i = 0; i < length; i++)
                {
                    otp[i] = validChars[buffer[i] % validChars.Length];
                }

                return new string(otp);
            }
        }
    }
}
