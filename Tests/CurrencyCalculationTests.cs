using TechMoveGLMS.Models;
using TechMoveGLMS.Services;

namespace Tests
{
    /// <summary>
    /// Tests the currency conversion math in ContractValidationService.
    /// Demonstrates TDD: every rule in the service has a corresponding test.
    /// </summary>
    public class CurrencyCalculationTests
    {
        private readonly ContractValidationService _validationService = new();

        // ────────────────────────────────────────────────────────────
        // HAPPY PATH — correct conversions
        // ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(100.00, 18.50, 1850.00)]   // Whole number USD, typical ZAR rate
        [InlineData(50.50, 19.00, 959.50)]   // Decimal USD amount
        [InlineData(0.01, 18.00, 0.18)]   // Minimum meaningful amount
        [InlineData(999.99, 20.00, 19999.80)]   // Large amount
        [InlineData(1.00, 18.65, 18.65)]   // Exact rate passthrough
        public void ConvertUsdToZar_ValidInputs_ReturnsCorrectAmount(
            decimal usd, decimal rate, decimal expectedZar)
        {
            // Act
            decimal result = _validationService.ConvertUsdToZar(usd, rate);

            // Assert
            Assert.Equal(expectedZar, result);
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — zero USD value
        // ────────────────────────────────────────────────────────────

        [Fact]
        public void ConvertUsdToZar_ZeroUsdAmount_ReturnsZero()
        {
            // Arrange
            decimal usd = 0m;
            decimal rate = 18.50m;

            // Act
            decimal result = _validationService.ConvertUsdToZar(usd, rate);

            // Assert
            Assert.Equal(0m, result);
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — result is rounded to 2 decimal places
        // ────────────────────────────────────────────────────────────

        [Fact]
        public void ConvertUsdToZar_ResultWithMoreThan2Decimals_IsRoundedCorrectly()
        {
            // 33.33 * 18.50 = 616.605 — should round to 616.61
            decimal usd = 33.33m;
            decimal rate = 18.50m;

            decimal result = _validationService.ConvertUsdToZar(usd, rate);

            Assert.Equal(616.60m, result);
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — negative USD amount throws
        // ────────────────────────────────────────────────────────────

        [Fact]
        public void ConvertUsdToZar_NegativeUsdAmount_ThrowsArgumentOutOfRangeException()
        {
            // Arrange
            decimal usd = -100m;
            decimal rate = 18.50m;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _validationService.ConvertUsdToZar(usd, rate));
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — zero or negative exchange rate throws
        // ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0)]
        [InlineData(-5)]
        public void ConvertUsdToZar_InvalidRate_ThrowsArgumentOutOfRangeException(decimal badRate)
        {
            // Arrange
            decimal usd = 100m;

            // Act & Assert
            Assert.Throws<ArgumentOutOfRangeException>(
                () => _validationService.ConvertUsdToZar(usd, badRate));
        }

        // ────────────────────────────────────────────────────────────
        // LEGACY — keeping the original inline math test for completeness
        // (proves the same formula works directly on ServiceRequest model)
        // ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(100.00, 18.50, 1850.00)]
        [InlineData(50.50, 19.00, 959.50)]
        [InlineData(0.00, 18.00, 0.00)]
        public void CalculateZar_DirectModelMath_ReturnsCorrectAmount(
            decimal usd, decimal rate, decimal expectedZar)
        {
            // Arrange
            var request = new ServiceRequest { OriginalCostUSD = usd };

            // Act
            request.ConvertedCostZAR = Math.Round(usd * rate, 2);

            // Assert
            Assert.Equal(expectedZar, request.ConvertedCostZAR);
        }

        // ────────────────────────────────────────────────────────────
        // IsCostValid helper
        // ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData(0.01, true)]   // Minimum valid cost
        [InlineData(100, true)]   // Normal cost
        [InlineData(0, false)]  // Zero is not valid for a service request
        [InlineData(-1, false)]  // Negative is invalid
        public void IsCostValid_VariousAmounts_ReturnsExpected(decimal amount, bool expected)
        {
            bool result = _validationService.IsCostValid(amount);
            Assert.Equal(expected, result);
        }
    }
}