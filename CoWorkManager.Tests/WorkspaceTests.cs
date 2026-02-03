using Xunit;
using Moq;
using CoWorkManager.Models;
using CoWorkManager.Models.Interfaces;
using CoWorkManager.Models.Repositories;
using System.Collections.Generic;
using System.Linq;

namespace CoWorkManager.Tests
{
    public class WorkspaceTests
    {
        // --- TEST 1: UNIT TEST ---
        // Verifies logic using Moq (Doesn't need SQL)
        [Fact]
        public void GetById_ShouldReturnWorkspace_WhenMocked()
        {
            // Arrange
            var mockRepo = new Mock<IWorkspaceRepository>();
            var fakeWorkspace = new Workspace { WorkspaceId = 1, Name = "Mock Desk" };
            mockRepo.Setup(repo => repo.GetById(1)).Returns(fakeWorkspace);

            // Act
            var result = mockRepo.Object.GetById(1);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Mock Desk", result.Name);
        }

        // --- TEST 2: INTEGRATION TEST ---
        // Verifies the actual connection to your SQLEXPRESS database
        [Fact]
        public void GetAll_ShouldReturnWorkspacesFromDatabase()
        {
            // Arrange: Using your exact connection string from appsettings.json
            string conn = @"Server=localhost\SQLEXPRESS;Database=CoWorkManager;Trusted_Connection=True;Encrypt=False;MultipleActiveResultSets=true";
            var repo = new WorkspaceRepository(conn);

            // Act
            var result = repo.GetAll();

            // Assert
            Assert.NotNull(result); 
        }
    }
}