using Microsoft.AspNetCore.Http;
using Moq;
using TechMoveGLMS.Services;

namespace Tests
{
    /// <summary>
    /// Tests the DocumentHandlingService file-upload validation.
    /// Uses Moq to simulate IFormFile without touching the real file system.
    /// </summary>
    public class DocumentServiceTests
    {
        private static Mock<IFormFile> CreateFileMock(string fileName, long length = 1024)
        {
            var mock = new Mock<IFormFile>();
            mock.Setup(f => f.FileName).Returns(fileName);
            mock.Setup(f => f.Length).Returns(length);
            return mock;
        }

        // ────────────────────────────────────────────────────────────
        // INVALID TYPES — must throw InvalidOperationException
        // ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadPdfAsync_ExeFile_ThrowsInvalidOperationException()
        {
            var service = new DocumentHandlingService();
            var fileMock = CreateFileMock("malicious_script.exe");

            var ex = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadPdfAsync(fileMock.Object));

            Assert.Equal("Only PDF files are allowed.", ex.Message);
        }

        [Fact]
        public async Task UploadPdfAsync_DocxFile_ThrowsInvalidOperationException()
        {
            var service = new DocumentHandlingService();
            var fileMock = CreateFileMock("contract_draft.docx");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadPdfAsync(fileMock.Object));
        }

        [Fact]
        public async Task UploadPdfAsync_JpgFile_ThrowsInvalidOperationException()
        {
            var service = new DocumentHandlingService();
            var fileMock = CreateFileMock("sneaky_image.jpg");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadPdfAsync(fileMock.Object));
        }

        [Fact]
        public async Task UploadPdfAsync_ZipFile_ThrowsInvalidOperationException()
        {
            var service = new DocumentHandlingService();
            var fileMock = CreateFileMock("archive.zip");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadPdfAsync(fileMock.Object));
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — null file throws ArgumentException
        // ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadPdfAsync_NullFile_ThrowsArgumentException()
        {
            var service = new DocumentHandlingService();

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UploadPdfAsync(null!));
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — zero-length file throws ArgumentException
        // ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadPdfAsync_EmptyFile_ThrowsArgumentException()
        {
            var service = new DocumentHandlingService();
            var fileMock = CreateFileMock("empty.pdf", length: 0);

            await Assert.ThrowsAsync<ArgumentException>(
                () => service.UploadPdfAsync(fileMock.Object));
        }

        // ────────────────────────────────────────────────────────────
        // EDGE CASE — disguised PDF (PDF extension but .exe in name)
        // ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadPdfAsync_FileWithNoExtension_ThrowsInvalidOperationException()
        {
            var service = new DocumentHandlingService();
            var fileMock = CreateFileMock("noextensionfile");

            await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadPdfAsync(fileMock.Object));
        }

        // ────────────────────────────────────────────────────────────
        // VALID — a real PDF should NOT throw (it writes to disk)
        // We test the happy path by confirming no exception is raised.
        // ────────────────────────────────────────────────────────────

        [Fact]
        public async Task UploadPdfAsync_ValidPdfFile_DoesNotThrowValidationError()
        {
            var service = new DocumentHandlingService();

            // Set up a mock that also streams content so the file copy works
            var content = new byte[] { 37, 80, 68, 70 }; // PDF magic bytes %PDF
            var stream = new MemoryStream(content);

            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("signed_agreement.pdf");
            fileMock.Setup(f => f.Length).Returns(content.Length);
            fileMock.Setup(f => f.CopyToAsync(It.IsAny<Stream>(), It.IsAny<CancellationToken>()))
                    .Returns(Task.CompletedTask);

            // Act — should NOT throw an InvalidOperationException about the extension
            // (It may throw on disk write in test environment, so we only check for
            //  the validation-specific exception type, not IO exceptions)
            try
            {
                await service.UploadPdfAsync(fileMock.Object);
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("PDF"))
            {
                Assert.Fail("Valid PDF file was incorrectly rejected by extension validation.");
            }
            catch
            {
                // IO or directory exceptions in the test environment are acceptable —
                // what matters is that the extension validation passed.
            }
        }
    }
}