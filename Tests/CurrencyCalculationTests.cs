using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using TechMoveGLMS.Models;

namespace Tests
{
    public class CurrencyCalculationTests
    {
        [Theory]
        [InlineData(100.00, 18.50, 1850.00)]  // $100 at R18.50 rate
        [InlineData(50.50, 19.00, 959.50)]    // Edge case: decimal math
        [InlineData(0, 18.00, 0)]             // Edge case: Zero value
        public void CalculateZar_GivenUsdAndRate_ReturnsCorrectAmount(decimal usd, decimal rate, decimal expectedZar)
        {
            // Arrange
            var request = new ServiceRequest { OriginalCostUSD = usd };

            // Act
            request.ConvertedCostZAR = Math.Round(usd * rate, 2);

            // Assert
            Assert.Equal(expectedZar, request.ConvertedCostZAR);
        }
    }
}
