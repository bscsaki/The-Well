using TheWell.API.Services;

namespace TheWell.Tests;

public class OtpServiceTests
{
    private readonly OtpService _sut = new();

    [Fact]
    public void Generate_Returns6DigitString()
    {
        var otp = _sut.Generate();
        Assert.Equal(6, otp.Length);
        Assert.True(int.TryParse(otp, out _));
    }

    [Fact]
    public void Hash_SameInput_ReturnsSameHash()
    {
        var hash1 = _sut.Hash("123456");
        var hash2 = _sut.Hash("123456");
        Assert.Equal(hash1, hash2);
    }

    [Fact]
    public void Verify_CorrectOtpNotExpired_ReturnsTrue()
    {
        var otp = "654321";
        var hash = _sut.Hash(otp);
        Assert.True(_sut.Verify(otp, hash, DateTime.UtcNow.AddMinutes(15)));
    }

    [Fact]
    public void Verify_CorrectOtpExpired_ReturnsFalse()
    {
        var otp = "654321";
        var hash = _sut.Hash(otp);
        Assert.False(_sut.Verify(otp, hash, DateTime.UtcNow.AddMinutes(-1)));
    }

    [Fact]
    public void Verify_WrongOtp_ReturnsFalse()
    {
        var hash = _sut.Hash("111111");
        Assert.False(_sut.Verify("222222", hash, DateTime.UtcNow.AddMinutes(15)));
    }
}
