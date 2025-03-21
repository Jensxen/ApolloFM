using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;
using Moq;
using FM.Domain.Entities;
using FM.Infrastructure.Repositories;
using FM.Domain.Interfaces;

namespace FM.Test.RepositoryTest
{
    public class UserRepositoryTest
    {
        private readonly Mock<IUserRepository> _mockUserRepository;
        private readonly List<User> _users;

        public UserRepositoryTest()
        {
            _mockUserRepository = new Mock<IUserRepository>();
            _users = new List<User>
            {
                new User { Id = "1", DisplayName = "First User", SpotifyUserId = "spotify1" },
                new User { Id = "2", DisplayName = "Second User", SpotifyUserId = "spotify2" }
            };
        }

        [Fact]
        public async Task GetAllUsers_ShouldReturnAllUsers()
        {
            // Arrange
            _mockUserRepository.Setup(repo => repo.GetAllUsersAsync()).ReturnsAsync(_users);

            // Act
            var result = await _mockUserRepository.Object.GetAllUsersAsync();

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetUserById_ShouldReturnCorrectUser()
        {
            // Arrange
            var userId = "1";
            _mockUserRepository.Setup(repo => repo.GetUserByIdAsync(userId)).ReturnsAsync(_users.First(p => p.Id == userId));

            // Act
            var result = await _mockUserRepository.Object.GetUserByIdAsync(userId);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(userId, result.Id);
        }

        [Fact]
        public async Task AddUser_ShouldAddUser()
        {
            // Arrange
            var newUser = new User { Id = "3", DisplayName = "Third User", SpotifyUserId = "spotify3" };
            _mockUserRepository.Setup(repo => repo.AddUserAsync(newUser)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _mockUserRepository.Object.AddUserAsync(newUser);

            // Assert
            _mockUserRepository.Verify(repo => repo.AddUserAsync(newUser), Times.Once);
        }

        [Fact]
        public async Task UpdateUser_ShouldUpdateUser()
        {
            // Arrange
            var userToUpdate = _users.First();
            userToUpdate.DisplayName = "Updated Name";
            _mockUserRepository.Setup(repo => repo.UpdateUserAsync(userToUpdate)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _mockUserRepository.Object.UpdateUserAsync(userToUpdate);

            // Assert
            _mockUserRepository.Verify(repo => repo.UpdateUserAsync(userToUpdate), Times.Once);
        }

        [Fact]
        public async Task DeleteUser_ShouldRemoveUser()
        {
            // Arrange
            var userId = "1";
            _mockUserRepository.Setup(repo => repo.DeleteUserAsync(userId)).Returns(Task.CompletedTask).Verifiable();

            // Act
            await _mockUserRepository.Object.DeleteUserAsync(userId);

            // Assert
            _mockUserRepository.Verify(repo => repo.DeleteUserAsync(userId), Times.Once);
        }
    }
}
