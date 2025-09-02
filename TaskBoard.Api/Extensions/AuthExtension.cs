using FluentValidation;
using Microsoft.EntityFrameworkCore;
using TaskBoard.Api.Models;

namespace TaskBoard.Extensions;

public static class AuthExtension
{
    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app, WebApplicationBuilder builder)
    {
        app.MapPost("/auth/register", async (RegisterRequest req, TaskBoardDbContext db,IValidator<RegisterRequest> validator, IPasswordService pwd, ITokenService tokens, CancellationToken ct) =>
        {
             var validationResult = await validator.ValidateAsync(req);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
                return Results.BadRequest("Email and password are required.");

            var email = req.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(u => u.Email == email, ct))
                return Results.Conflict("Email already registered.");

            var user = new AppUser
            {
                Email = email,
                PasswordHash = pwd.Hash(req.Password),
                DisplayName = req.DisplayName
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            var tr = tokens.CreateTokens(user);

            // store refresh token hash
            db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokens.Hash(tr.RefreshToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.Parse(builder.Configuration["Jwt:RefreshTokenDays"]!))
            });
            await db.SaveChangesAsync(ct);

            return Results.Ok(tr);
        })
        .WithTags("Auth");

        app.MapPost("/auth/login", async (LoginRequest req, TaskBoardDbContext db, IPasswordService pwd, ITokenService tokens,
        IValidator<LoginRequest> validator, CancellationToken ct) =>
        {
            var validationResult = await validator.ValidateAsync(req);

            if (!validationResult.IsValid)
            {
                return Results.BadRequest(validationResult.Errors);
            }
            var email = (req.Email ?? "").Trim().ToLowerInvariant();
            var user = await db.Users.Include(u => u.RefreshTokens).FirstOrDefaultAsync(u => u.Email == email, ct);
            if (user is null || !pwd.Verify(user.PasswordHash, req.Password))
                return Results.Unauthorized();

            var tr = tokens.CreateTokens(user);

            // invalidate oldest/expired tokens optionally here…

            db.RefreshTokens.Add(new RefreshToken
            {
                UserId = user.Id,
                TokenHash = tokens.Hash(tr.RefreshToken),
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.Parse(builder.Configuration["Jwt:RefreshTokenDays"]!))
            });
            await db.SaveChangesAsync(ct);

            return Results.Ok(tr);
        })
        .WithTags("Auth");

        app.MapPost("/auth/refresh", async (RefreshRequest req, TaskBoardDbContext db, ITokenService tokens, CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken)) return Results.BadRequest("Missing refresh token.");
            var hash = tokens.Hash(req.RefreshToken);

            var token = await db.RefreshTokens.Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == hash && rt.ExpiresAt > DateTimeOffset.UtcNow, ct);

            if (token is null || !token.IsActive)
                return Results.Unauthorized();

            // rotate: revoke old and issue new
            token.RevokedAt = DateTimeOffset.UtcNow;

            var tr = tokens.CreateTokens(token.User);
            var newHash = tokens.Hash(tr.RefreshToken);
            db.RefreshTokens.Add(new RefreshToken
            {
                UserId = token.UserId,
                TokenHash = newHash,
                ExpiresAt = DateTimeOffset.UtcNow.AddDays(int.Parse(builder.Configuration["Jwt:RefreshTokenDays"]!)),
                ReplacedByTokenHash = null
            });
            await db.SaveChangesAsync(ct);

            return Results.Ok(tr);
        })
        .WithTags("Auth");

        app.MapPost("/auth/revoke", async (RefreshRequest req, TaskBoardDbContext db, ITokenService tokens, CancellationToken ct) =>
        {
            var hash = tokens.Hash(req.RefreshToken);
            var token = await db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash, ct);
            if (token is null) return Results.NotFound();
            token.RevokedAt = DateTimeOffset.UtcNow;
            await db.SaveChangesAsync(ct);
            return Results.NoContent();
        })
        .WithTags("Auth");

        return app;
    }
}
