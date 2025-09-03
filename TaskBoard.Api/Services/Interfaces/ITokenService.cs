using TaskBoard.Api.Models;

public interface ITokenService
{
    public TokenResponse CreateTokens(AppUser user);
    public string Hash(string input);
}
