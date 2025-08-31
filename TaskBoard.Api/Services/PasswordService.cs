using Microsoft.AspNetCore.Identity;
using TaskBoard.Api.Models;

public sealed class PasswordService : IPasswordService
{
    private readonly PasswordHasher<AppUser> _hasher = new();
    public string Hash(string password) => _hasher.HashPassword(new AppUser(), password);
    public bool Verify(string hashed, string password) =>
        _hasher.VerifyHashedPassword(new AppUser(), hashed, password) is not PasswordVerificationResult.Failed;
}
