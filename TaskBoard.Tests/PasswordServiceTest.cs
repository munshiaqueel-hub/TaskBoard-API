using NUnit.Framework;

namespace TaskBoard.Api.Tests
{
    [TestFixture]
    public class PasswordServiceTests
    {
        private PasswordService _passwordService;

        [SetUp]
        public void Setup()
        {
            _passwordService = new PasswordService();
        }

        [Test]
        public void Hash_ShouldReturnDifferentValueThanPlainPassword()
        {
            // Arrange
            var password = "MyStrongPassword!123";

            // Act
            var hashed = _passwordService.Hash(password);

            // Assert
            Assert.That(hashed, Is.Not.EqualTo(password), "Hashed password should not equal raw password");
            Assert.That(hashed, Is.Not.Null.And.Not.Empty, "Hashed password should not be null or empty");
        }

        [Test]
        public void Verify_ShouldReturnTrue_WhenPasswordMatchesHash()
        {
            // Arrange
            var password = "MyStrongPassword!123";
            var hashed = _passwordService.Hash(password);

            // Act
            var result = _passwordService.Verify(hashed, password);

            // Assert
            Assert.That(result, Is.True, "Verification should succeed with correct password");
        }

        [Test]
        public void Verify_ShouldReturnFalse_WhenPasswordDoesNotMatchHash()
        {
            // Arrange
            var password = "MyStrongPassword!123";
            var wrongPassword = "WrongPassword!999";
            var hashed = _passwordService.Hash(password);

            // Act
            var result = _passwordService.Verify(hashed, wrongPassword);

            // Assert
            Assert.That(result, Is.False, "Verification should fail with incorrect password");
        }

        [Test]
        public void Verify_ShouldReturnFalse_WhenHashIsValidButPasswordWrong()
        {
            // Arrange
            var hashed = _passwordService.Hash("CorrectPassword123");
            var wrongPassword = "WrongPassword999";

            // Act
            var result = _passwordService.Verify(hashed, wrongPassword);

            // Assert
            Assert.That(result, Is.False);
        }
    }
}
