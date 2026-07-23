using BookingSystem.Domain.Common.Errors;
using BookingSystem.Domain.Users.ValueObjects;

namespace BookingSystem.Tests.Domain.Users.ValueObjects;

public class UsernameTests
{
    [Theory]
    [InlineData("f+ef+efe")]
    [InlineData("@Agrgr#$4aaa--a")]
    [InlineData("")]
    [InlineData("kk")]
    public void Create_WrongUsername_ReturnsError(string username)
    {
        var res = Username.Create(username);
        res.ShouldContain<ValidationError>();
    }
    
    [Theory]
    [InlineData("KaiAngel")]
    [InlineData("demon_v_tapkah")]
    [InlineData("sergey_67")]
    public void Create_CorrectUsername_ReturnsUsername(string username)
    {
        var res = Username.Create(username);
        res.ShouldBeSuccess();
    }
}