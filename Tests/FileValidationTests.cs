namespace Tests
{
    /// <summary>
    /// Tests the file extension validation logic in isolation.
    /// These are pure unit tests — no mocks or I/O required.
    /// </summary>
    public class FileValidationTests
    {
        // Helper — mirrors the logic in DocumentHandlingService
        private static bool IsValidPdf(string? fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return false;
            return Path.GetExtension(fileName).ToLowerInvariant() == ".pdf";
        }

        // ────────────────────────────────────────────────────────────
        // VALID EXTENSIONS
        // ────────────────────────────────────────────────────────────

        [Fact]
        public void ValidateExtension_LowercasePdf_ReturnsTrue()
        {
            Assert.True(IsValidPdf("signed_contract.pdf"));
        }

        [Fact]
        public void ValidateExtension_UppercasePdf_ReturnsTrue()
        {
            // Extension check is case-insensitive
            Assert.True(IsValidPdf("SIGNED_CONTRACT.PDF"));
        }

        [Fact]
        public void ValidateExtension_MixedCasePdf_ReturnsTrue()
        {
            Assert.True(IsValidPdf("Agreement.Pdf"));
        }

        // ────────────────────────────────────────────────────────────
        // INVALID EXTENSIONS
        // ────────────────────────────────────────────────────────────

        [Theory]
        [InlineData("malicious_script.exe")]
        [InlineData("spreadsheet.xlsx")]
        [InlineData("document.docx")]
        [InlineData("image.jpg")]
        [InlineData("image.png")]
        [InlineData("archive.zip")]
        [InlineData("script.sh")]
        [InlineData("script.bat")]
        public void ValidateExtension_ForbiddenExtensions_ReturnsFalse(string fileName)
        {
            Assert.False(IsValidPdf(fileName));
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASES — null, empty, no extension
        // ────────────────────────────────────────────────────────────

        [Fact]
        public void ValidateExtension_NullFilename_ReturnsFalse()
        {
            Assert.False(IsValidPdf(null));
        }

        [Fact]
        public void ValidateExtension_EmptyString_ReturnsFalse()
        {
            Assert.False(IsValidPdf(string.Empty));
        }

        [Fact]
        public void ValidateExtension_WhiteSpaceOnly_ReturnsFalse()
        {
            Assert.False(IsValidPdf("   "));
        }

        [Fact]
        public void ValidateExtension_NoExtension_ReturnsFalse()
        {
            Assert.False(IsValidPdf("contractfile"));
        }

        [Fact]
        public void ValidateExtension_PdfWordInNameButWrongExtension_ReturnsFalse()
        {
            // Attacker names file "contract.pdf.exe" — must still be rejected
            Assert.False(IsValidPdf("contract.pdf.exe"));
        }

        [Fact]
        public void ValidateExtension_DotOnlyExtension_ReturnsFalse()
        {
            Assert.False(IsValidPdf("file."));
        }
    }
}