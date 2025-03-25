using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FM.Domain.Entities;
using FM.Infrastructure.Repositories;
using FM.Application.Interfaces;

namespace FM.Test.RepositoryTest
{
    public class SubForumTests
    {
        private readonly Mock<ISubForumRepository> _mockSubForumRepository;
        private readonly List<SubForum> _subForums;

        public SubForumTests()
        {
            _mockSubForumRepository = new Mock<ISubForumRepository>();
            _subForums = new List<SubForum>
            {
                new SubForum { Id = 1, Name = "First SubForum", Description = "Description of the first subforum" },
                new SubForum { Id = 2, Name = "Second SubForum", Description = "Description of the second subforum" }
            };
        }

        [Fact]
        public async Task GetAllSubForums_ShouldReturnAllSubForums()
        {
            // Arrange
            _mockSubForumRepository.Setup(repo => repo.GetAllSubForumsAsync()).ReturnsAsync(_subForums);

            // Act
            var result = await _mockSubForumRepository.Object.GetAllSubForumsAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetSubForumById_ShouldReturnCorrectSubForum()
        {
            // Arrange
            var subForumId = 1;
            _mockSubForumRepository.Setup(repo => repo.GetSubForumByIdAsync(subForumId)).ReturnsAsync(_subForums.First(p => p.Id == subForumId));

            // Act
            var result = await _mockSubForumRepository.Object.GetSubForumByIdAsync(subForumId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(subForumId, result.Id);
        }

        [Fact]
        public async Task AddSubForum_ShouldAddSubForum()
        {
            // Arrange
            var newSubForum = new SubForum { Id = 3, Name = "Third SubForum", Description = "Description of the third subforum" };
            _mockSubForumRepository.Setup(repo => repo.AddSubForumAsync(newSubForum)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _mockSubForumRepository.Object.AddSubForumAsync(newSubForum);

            // Assert
            _mockSubForumRepository.Verify(repo => repo.AddSubForumAsync(newSubForum), Times.Once);
        }

        [Fact]
        public async Task UpdateSubForum_ShouldUpdateSubForum()
        {
            // Arrange
            var subForumToUpdate = _subForums.First();
            subForumToUpdate.Name = "Updated Name";
            _mockSubForumRepository.Setup(repo => repo.UpdateSubForumAsync(subForumToUpdate)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _mockSubForumRepository.Object.UpdateSubForumAsync(subForumToUpdate);

            // Assert
            _mockSubForumRepository.Verify(repo => repo.UpdateSubForumAsync(subForumToUpdate), Times.Once);
        }

        [Fact]
        public async Task DeleteSubForum_ShouldRemoveSubForum()
        {
            // Arrange
            var subForumId = 1;
            _mockSubForumRepository.Setup(repo => repo.DeleteSubForumAsync(subForumId)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _mockSubForumRepository.Object.DeleteSubForumAsync(subForumId);

            // Assert
            _mockSubForumRepository.Verify(repo => repo.DeleteSubForumAsync(subForumId), Times.Once);
        }
    }
}

