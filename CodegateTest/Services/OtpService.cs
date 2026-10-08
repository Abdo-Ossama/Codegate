namespace CodegateTest.Services
{
    using CodegateTest.Services.IServices;
    using System.Security.Cryptography;
    using System.Text;

    public class OtpService : IOtpService
    {
        public string GenerateOtp()
        {
            return RandomNumberGenerator
                .GetInt32(1000, 10000)
                .ToString();
        }

        public string HashOtp(string otp)
        {
            var hash = SHA256.HashData(
                Encoding.UTF8.GetBytes(otp));

            return Convert.ToBase64String(hash);
        }

        public bool VerifyOtp(string otp, string hash)
        {
            var otpHash = HashOtp(otp);

            return CryptographicOperations.FixedTimeEquals(
                Convert.FromBase64String(otpHash),
                Convert.FromBase64String(hash));
        }
    }
}
