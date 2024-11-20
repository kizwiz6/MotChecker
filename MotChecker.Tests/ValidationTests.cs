using Xunit;
using FluentAssertions;
using System.ComponentModel.DataAnnotations;
using static MotChecker.Pages.Home;

namespace MotChecker.Tests
{
    /// <summary>
    /// Tests for registration number validation
    /// </summary>
    public class ValidationTests
    {
        /// <summary>
        /// Tests various registration number formats for validity
        /// </summary>
        /// <param name="registration">Registration number to test</param>
        /// <param name="shouldBeValid">Expected validation result</param>
        [Theory]
        [InlineData("AB12CDE", true)]        // Valid format
        [InlineData("AB12", true)]           // Valid shorter format
        [InlineData("1234ABC", true)]        // Valid numeric start
        [InlineData("AB 12 CDE", true)]      // Valid with spaces
        [InlineData("LB 11 WXA", true)]      // Valid with spaces
        [InlineData("", false)]              // Empty string
        [InlineData("AB12CDEFGHIJK", false)] // Too long (>14 chars)
        [InlineData("AB!2CDE", false)]       // Invalid character
        [InlineData("  ", false)]            // Only whitespace
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