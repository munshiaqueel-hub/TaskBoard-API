using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using TaskBoard.Api.Models;

namespace TaskBoard.Tests.Services
{
    [TestFixture]
    public class TokenServiceTests
    {
        private IConfiguration _config = null!;
        private TokenService _service = null!;

        [SetUp]
        public void Setup()
        {
            // Minimal JWT config for testing
            var inMemorySettings = new Dictionary<string, string?>
            {
                { "Jwt:Issuer", "TestIssuer" },
                { "Jwt:Audience", "TestAudience" },
                { "Jwt:Key", "c4vdkg8OtND/KqVwTs5blIoKs54ryidV0EMdrk2gQU8=" }, // must be at least 256-bit
                { "Jwt:AccessTokenMinutes", "30" },
                { "Jwt:RefreshTokenDays", "7" }
            };

            _config = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();

            _service = new TokenService(_config);
        }

        [Test]
        public void CreateTokens_ShouldReturnValidJwtAndRefreshToken()
        {
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "test@example.com",
                DisplayName = "Test User"
            };

            var result = ((ITokenService)_service).CreateTokens(user);

            Assert.That(result, Is.Not.Null);
            Assert.That(result.AccessToken, Is.Not.Null.And.Not.Empty);
            Assert.That(result.RefreshToken, Is.Not.Null.And.Not.Empty);

            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(result.AccessToken);

            Assert.That(token.Issuer, Is.EqualTo("TestIssuer"));
            Assert.That(token.Audiences.First(), Is.EqualTo("TestAudience"));
            Assert.That(token.Claims.First(c => c.Type == JwtRegisteredClaimNames.Email).Value,
                Is.EqualTo("test@example.com"));
            Assert.That(token.ValidTo, Is.EqualTo(result.ExpiresAt.UtcDateTime).Within(TimeSpan.FromSeconds(5)));
        }

        [Test]
        public void CreateTokens_ShouldGenerateStrongRefreshToken()
        {
            var user = new AppUser
            {
                Id = Guid.NewGuid(),
                Email = "user@domain.com",
                DisplayName = "Name"
            };

            var result = ((ITokenService)_service).CreateTokens(user);

            var bytes = Convert.FromBase64String(result.RefreshToken);
            Assert.That(bytes.Length, Is.EqualTo(64), "Refresh token should be 64 bytes");
        }

        [Test]
        public void Hash_ShouldReturnExpectedSha256()
        {
            var input = "hello";
            var expected = "2CF24DBA5FB0A30E26E83B2AC5B9E29E1B161E5C1FA7425E73043362938B9824";

            var actual = _service.Hash(input);

            Assert.That(actual, Is.EqualTo(expected));
        }

        // 🔴 Negative Tests

        [Test]
        public void CreateTokens_ShouldThrow_WhenJwtKeyMissing()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Issuer", "Issuer" },
                    { "Jwt:Audience", "Audience" },
                    { "Jwt:AccessTokenMinutes", "30" },
                    { "Jwt:RefreshTokenDays", "7" }
                })
                .Build();

            var service = new TokenService(badConfig);

            var user = new AppUser { Id = Guid.NewGuid(), Email = "missing@key.com" };

            Assert.Throws<ArgumentNullException>(() =>
                ((ITokenService)service).CreateTokens(user));
        }

        [Test]
        public void CreateTokens_ShouldThrow_WhenAccessTokenMinutesInvalid()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Issuer", "Issuer" },
                    { "Jwt:Audience", "Audience" },
                    { "Jwt:Key", "SuperSecretTestKey123!SuperSecretTestKey123!" },
                    { "Jwt:AccessTokenMinutes", "INVALID" }, // 👈 invalid value
                    { "Jwt:RefreshTokenDays", "7" }
                })
                .Build();

            var service = new TokenService(badConfig);

            var user = new AppUser { Id = Guid.NewGuid(), Email = "invalid@minutes.com" };

            Assert.Throws<FormatException>(() =>
                ((ITokenService)service).CreateTokens(user));
        }

        [Test]
        public void CreateTokens_ShouldThrow_WhenRefreshTokenDaysInvalid()
        {
            var badConfig = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "Jwt:Issuer", "Issuer" },
                    { "Jwt:Audience", "Audience" },
                    { "Jwt:Key", "SuperSecretTestKey123!SuperSecretTestKey123!" },
                    { "Jwt:AccessTokenMinutes", "30" },
                    { "Jwt:RefreshTokenDays", "NOT_A_NUMBER" } // 👈 invalid
                })
                .Build();

            var service = new TokenService(badConfig);

            var user = new AppUser { Id = Guid.NewGuid(), Email = "invalid@refresh.com" };

            Assert.Throws<FormatException>(() =>
                ((ITokenService)service).CreateTokens(user));
        }

        [Test]
        public void Hash_ShouldThrow_WhenInputIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => _service.Hash(null!));
        }
    }
}
