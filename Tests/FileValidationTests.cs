using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using System.IO;

namespace Tests
{
    public class FileValidationTests
    {
        [Fact]
        public void ValidateFileExtension_GivenPdf_ReturnsTrue()
        {
            // Arrange
            string filename = "signed_contract.pdf";

            // Act
            bool isValid = Path.GetExtension(filename).ToLower() == ".pdf";

            // Assert
            Assert.True(isValid);
        }

        [Fact]
        public void ValidateFileExtension_GivenExe_ReturnsFalse()
        {
            // Arrange
            string filename = "malicious_script.exe";

            // Act
            bool isValid = Path.GetExtension(filename).ToLower() == ".pdf";

            // Assert
            Assert.False(isValid);
        }
    }
}