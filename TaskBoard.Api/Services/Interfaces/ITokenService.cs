using TaskBoard.Api.Models;
public interface ITokenService
{
    TokenResponse CreateTokens(AppUser user);
    string Hash(string input);
}
