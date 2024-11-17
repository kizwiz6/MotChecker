using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using static MotChecker.Pages.Home;

namespace MotChecker.Tests
{
    public class ValidationTests
    {
        [Theory]
        [InlineData("AB12CDE", true)]  // Valid format
        [InlineData("AB12", true)]     // Valid shorter format
        [InlineData("1234ABC", true)]  // Valid numeric start
        [InlineData("", false)]        // Empty string
        [InlineData("AB12CDEFG", false)] // Too long
        [InlineData("AB!2CDE", false)]   // Invalid character
        [InlineData("  ", false)]        // Whitespace
        public void Registration_Validation_Should_Work_As_Expected(string registration, bool shouldBeValid)
        {
            // Arrange
            var model = new SearchModel { Registration = registration };
            var context = new ValidationContext(model);
            var results = new List<ValidationResult>();

            // Act
            var isValid = Validator.TryValidateObject(model, context, results, true);

            // Assert
            isValid.Should().Be(shouldBeValid);

        }
    }
}