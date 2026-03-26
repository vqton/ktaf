using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AMS.Application.DTOs;
using AMS.Application.Interfaces;
using AMS.Application.Services;
using AMS.Domain.Entities;
using AMS.Domain.Interfaces;
using Moq;
using Xunit;

namespace AMS.Application.Tests.Unit
{
    public class AuthorizationServiceTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly Mock<IRoleRepository> _roleRepositoryMock;
        private readonly Mock<IADGroupRepository> _adGroupRepositoryMock;
        private readonly Mock<IPermissionRepository> _permissionRepositoryMock;
        private readonly Mock<IUserRoleRepository> _userRoleRepositoryMock;
        private readonly Mock<IUserADGroupRepository> _userADGroupRepositoryMock;
        private readonly Mock<IADGroupRoleRepository> _adGroupRoleRepositoryMock;
        private readonly Mock<IRolePermissionRepository> _rolePermissionRepositoryMock;
        private readonly AuthorizationService _service;

        public AuthorizationServiceTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _roleRepositoryMock = new Mock<IRoleRepository>();
            _adGroupRepositoryMock = new Mock<IADGroupRepository>();
            _permissionRepositoryMock = new Mock<IPermissionRepository>();
            _userRoleRepositoryMock = new Mock<IUserRoleRepository>();
            _userADGroupRepositoryMock = new Mock<IUserADGroupRepository>();
            _adGroupRoleRepositoryMock = new Mock<IADGroupRoleRepository>();
            _rolePermissionRepositoryMock = new Mock<IRolePermissionRepository>();

            _service = new AuthorizationService(
                _userRepositoryMock.Object,
                _roleRepositoryMock.Object,
                _adGroupRepositoryMock.Object,
                _permissionRepositoryMock.Object,
                _userRoleRepositoryMock.Object,
                _userADGroupRepositoryMock.Object,
                _adGroupRoleRepositoryMock.Object,
                _rolePermissionRepositoryMock.Object);
        }

        [Fact]
        public async Task GetCurrentUserAsync_ReturnsNull_AsNotImplemented()
        {
            // Act
            var result = await _service.GetCurrentUserAsync();

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserRolesAsync_ReturnsEmptyList_WhenNoRolesExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRoleRepositoryMock.Setup(ur => ur.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserRole>());

            // Act
            var result = await _service.GetUserRolesAsync(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetUserPermissionsAsync_ReturnsEmptySet_WhenNoRolesOrGroups()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRoleRepositoryMock.Setup(ur => ur.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserRole>());
            _userADGroupRepositoryMock.Setup(uag => uag.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserADGroup>());

            // Act
            var result = await _service.GetUserPermissionsAsync(userId);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsFailure_WhenUsernameIsEmpty()
        {
            // Arrange
            var dto = new CreateUserDto { Username = "" };

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Username is required.", result.Errors);
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsFailure_WhenUsernameAlreadyExists()
        {
            // Arrange
            var dto = new CreateUserDto { Username = "existinguser" };
            _userRepositoryMock.Setup(u => u.GetByUsernameAsync(dto.Username, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = Guid.NewGuid(), Username = dto.Username });

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Contains("Username already exists.", result.Errors);
        }

        [Fact]
        public async Task CreateUserAsync_ReturnsSuccess_WhenValidDto()
        {
            // Arrange
            var dto = new CreateUserDto
            {
                Username = "newuser",
                DisplayName = "New User",
                Email = "newuser@example.com"
            };

            _userRepositoryMock.Setup(u => u.GetByUsernameAsync(dto.Username, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);
            _userRepositoryMock.Setup(u => u.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
                .Returns((User user, CancellationToken cancellationToken) => Task.FromResult(user));
            _userRoleRepositoryMock.Setup(ur => ur.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                .Returns((UserRole userRole, CancellationToken cancellationToken) => Task.CompletedTask);

            // Act
            var result = await _service.CreateUserAsync(dto);

            // Assert
            Assert.True(result.IsSuccess);
            Assert.NotNull(result.Data);
            Assert.Equal(dto.Username, result.Data.Username);
        }

        [Fact]
        public async Task AssignRoleAsync_ReturnsFailure_WhenUserNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            _userRepositoryMock.Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _service.AssignRoleAsync(userId, roleId, "admin");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("User not found.", result.Errors[0]);
        }

        [Fact]
        public async Task AssignRoleAsync_ReturnsFailure_WhenRoleNotFound()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            _userRepositoryMock.Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId });
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Role)null);

            // Act
            var result = await _service.AssignRoleAsync(userId, roleId, "admin");

            // Assert
            Assert.False(result.IsSuccess);
            Assert.Equal("Role not found.", result.Errors[0]);
        }

        [Fact]
        public async Task AssignRoleAsync_ReturnsSuccess_WhenValidParameters()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var roleId = Guid.NewGuid();
            _userRepositoryMock.Setup(u => u.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new User { Id = userId });
            _roleRepositoryMock.Setup(r => r.GetByIdAsync(roleId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new Role { Id = roleId });
            _userRoleRepositoryMock.Setup(ur => ur.AddAsync(It.IsAny<UserRole>(), It.IsAny<CancellationToken>()))
                .Returns((UserRole userRole, CancellationToken cancellationToken) => Task.CompletedTask);

            // Act
            var result = await _service.AssignRoleAsync(userId, roleId, "admin");

            // Assert
            Assert.True(result.IsSuccess);
        }

        [Fact]
        public async Task HasPermissionAsync_ReturnsFalse_WhenNoPermissions()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _userRoleRepositoryMock.Setup(ur => ur.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserRole>());
            _userADGroupRepositoryMock.Setup(uag => uag.GetByUserIdAsync(userId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<UserADGroup>());

            // Act
            var result = await _service.HasPermissionAsync(userId, "some.permission");

            // Assert
            Assert.False(result);
        }
    }
}