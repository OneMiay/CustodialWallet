using CustodialWallet.DTOs;
using CustodialWallet.Models;
using CustodialWallet.Repositories;
using CustodialWallet.Services;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustodialWallet.Tests;

public class UserServiceTests
{
    private static UserService CreateService(Mock<IUserRepository> repoMock)
    {
        var logger = Mock.Of<ILogger<UserService>>();
        return new UserService(repoMock.Object, logger);
    }

    [Fact]
    public async Task CreateUserAsync_Should_ReturnError_When_EmailExists()
    {
        // Arrange
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = Guid.NewGuid(), Email = "exists@example.com", Balance = 0m });
        var service = CreateService(repo);

        // Act
        var result = await service.CreateUserAsync(new CreateUserDto { Email = "exists@example.com" });

        // Assert
        result.Success.Should().BeFalse();
        result.Error.Should().Be("Email already exists");
        result.Result.Should().BeNull();
        repo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateUserAsync_Should_Create_When_EmailNotExists()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByEmailAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        repo.Setup(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User u, CancellationToken _) => u);
        var service = CreateService(repo);

        var result = await service.CreateUserAsync(new CreateUserDto { Email = "new@example.com" });

        result.Success.Should().BeTrue();
        result.Error.Should().BeNull();
        result.Result.Should().NotBeNull();
        result.Result!.Email.Should().Be("new@example.com");
        result.Result!.Balance.Should().Be(0m);
        repo.Verify(r => r.CreateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBalanceAsync_Should_ReturnNull_When_UserNotFound()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var service = CreateService(repo);

        var result = await service.GetBalanceAsync(Guid.NewGuid());

        result.Should().BeNull();
    }

    [Fact]
    public async Task GetBalanceAsync_Should_ReturnBalance_When_UserExists()
    {
        var userId = Guid.NewGuid();
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = userId, Email = "a@b.com", Balance = 12.34m });
        var service = CreateService(repo);

        var result = await service.GetBalanceAsync(userId);

        result.Should().NotBeNull();
        result!.UserId.Should().Be(userId);
        result.Balance.Should().Be(12.34m);
    }

    [Fact]
    public async Task DepositAsync_Should_ReturnNull_When_UserNotFound()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var service = CreateService(repo);

        var result = await service.DepositAsync(Guid.NewGuid(), 10m);

        result.Should().BeNull();
    }

    [Fact]
    public async Task DepositAsync_Should_IncreaseBalance_And_Save()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "a@b.com", Balance = 5m };
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repo.Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(repo);

        var result = await service.DepositAsync(userId, 2.5m);

        result.Should().NotBeNull();
        result!.Balance.Should().Be(7.5m);
        repo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Balance == 7.5m), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task WithdrawAsync_Should_ReturnNotFound_When_UserMissing()
    {
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);
        var service = CreateService(repo);

        var (success, error, result) = await service.WithdrawAsync(Guid.NewGuid(), 1m);

        success.Should().BeFalse();
        error.Should().Be("User not found");
        result.Should().BeNull();
    }

    [Fact]
    public async Task WithdrawAsync_Should_ReturnInsufficientFunds_When_NotEnoughBalance()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "a@b.com", Balance = 1m };
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        var service = CreateService(repo);

        var (success, error, result) = await service.WithdrawAsync(userId, 2m);

        success.Should().BeFalse();
        error.Should().Be("Insufficient funds");
        result.Should().BeNull();
        repo.Verify(r => r.UpdateAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task WithdrawAsync_Should_DecreaseBalance_And_Save()
    {
        var userId = Guid.NewGuid();
        var user = new User { Id = userId, Email = "a@b.com", Balance = 10m };
        var repo = new Mock<IUserRepository>();
        repo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);
        repo.Setup(r => r.UpdateAsync(user, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);
        var service = CreateService(repo);

        var (success, error, result) = await service.WithdrawAsync(userId, 4m);

        success.Should().BeTrue();
        error.Should().BeNull();
        result!.Balance.Should().Be(6m);
        repo.Verify(r => r.UpdateAsync(It.Is<User>(u => u.Balance == 6m), It.IsAny<CancellationToken>()), Times.Once);
    }
}





