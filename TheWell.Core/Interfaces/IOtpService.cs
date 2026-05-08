namespace TheWell.Core.Interfaces;

public interface IOtpService
{
    string Generate();
    string Hash(string otp);
    bool Verify(string otp, string storedHash, DateTime expiresAt);
}
