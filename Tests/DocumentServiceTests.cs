using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechMoveGLMS.Services;
using Microsoft.AspNetCore.Http;
using Moq;
using Xunit;

namespace Tests
{
    public class DocumentServiceTests
    {
        [Fact]
        public async Task UploadPdfAsync_GivenExeFile_ThrowsInvalidOperationException()
        {
            // Arrange
            var service = new DocumentHandlingService();
            var fileMock = new Mock<IFormFile>();
            fileMock.Setup(f => f.FileName).Returns("malicious_script.exe");
            fileMock.Setup(f => f.Length).Returns(1024);

            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(
                () => service.UploadPdfAsync(fileMock.Object));

            Assert.Equal("Only PDF files are allowed.", exception.Message);
        }
    }
}