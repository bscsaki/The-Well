using System.Security.Cryptography;
using System.Text;
using TheWell.Core.Interfaces;

namespace TheWell.API.Services;

public class OtpService : IOtpService
{
    public string Generate() =>
        Random.Shared.Next(100000, 999999).ToString();

    public string Hash(string otp)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(otp));
        return Convert.ToHexString(bytes);
    }

    public bool Verify(string otp, string storedHash, DateTime expiresAt)
    {
        if (DateTime.UtcNow > expiresAt) return false;
        return Hash(otp) == storedHash;
    }
}
