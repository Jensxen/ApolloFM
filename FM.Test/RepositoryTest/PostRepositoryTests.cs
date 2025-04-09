//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using Xunit;
//using Moq;
//using FM.Domain.Entities;
//using FM.Infrastructure.Repositories;
//using FM.Application.Interfaces.IRepositories;

//namespace FM.Test.RepositoryTest
//{
//    public class PostRepositoryTests
//    {
//        private readonly Mock<IPostRepository> _mockPostRepository;
//        private readonly List<Post> _posts;

//        public PostRepositoryTests()
//        {
//            _mockPostRepository = new Mock<IPostRepository>();
//            _posts = new List<Post>
//            {
//                new Post { Id = 1, Title = "First Post", Content = "Content of the first post" },
//                new Post { Id = 2, Title = "Second Post", Content = "Content of the second post" }
//            };
//        }

//        [Fact]
//        public async Task GetAllPosts_ShouldReturnAllPosts()
//        {
//            // Arrange
//            _mockPostRepository.Setup(repo => repo.GetAllPostsAsync()).ReturnsAsync(_posts);

//            // Act
//            var result = await _mockPostRepository.Object.GetAllPostsAsync();

//            // Assert
//            Assert.Equal(2, result.Count());
//        }

//        [Fact]
//        public async Task GetPostById_ShouldReturnCorrectPost()
//        {
//            // Arrange
//            var postId = 1;
//            _mockPostRepository.Setup(repo => repo.GetPostByIdAsync(postId)).ReturnsAsync(_posts.First(p => p.Id == postId));

//            // Act
//            var result = await _mockPostRepository.Object.GetPostByIdAsync(postId);

//            // Assert
//            Assert.NotNull(result);
//            Assert.Equal(postId, result.Id);
//        }

//        [Fact]
//        public async Task AddPost_ShouldAddPost()
//        {
//            // Arrange
//            var newPost = new Post { Id = 3, Title = "Third Post", Content = "Content of the third post" };
//            _mockPostRepository.Setup(repo => repo.AddPostAsync(newPost)).Returns(Task.CompletedTask).Verifiable();

//            // Act
//            await _mockPostRepository.Object.AddPostAsync(newPost);

//            // Assert
//            _mockPostRepository.Verify(repo => repo.AddPostAsync(newPost), Times.Once);
//        }

//        [Fact]
//        public async Task UpdatePost_ShouldUpdatePost()
//        {
//            // Arrange
//            var postToUpdate = _posts.First();
//            postToUpdate.Title = "Updated Title";
//            _mockPostRepository.Setup(repo => repo.UpdatePostAsync(postToUpdate)).Returns(Task.CompletedTask).Verifiable();

//            // Act
//            await _mockPostRepository.Object.UpdatePostAsync(postToUpdate);

//            // Assert
//            _mockPostRepository.Verify(repo => repo.UpdatePostAsync(postToUpdate), Times.Once);
//        }

//        [Fact]
//        public async Task DeletePost_ShouldRemovePost()
//        {
//            // Arrange
//            var postId = 1;
//            _mockPostRepository.Setup(repo => repo.DeletePostAsync(postId)).Returns(Task.CompletedTask).Verifiable();

//            // Act
//            await _mockPostRepository.Object.DeletePostAsync(postId);

//            // Assert
//            _mockPostRepository.Verify(repo => repo.DeletePostAsync(postId), Times.Once);
//        }
//    }
//}
