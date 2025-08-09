using CustodialWallet.Controllers;
using CustodialWallet.DTOs;
using CustodialWallet.Services;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace CustodialWallet.Tests;

public class UsersControllerTests
{
    private static UsersController CreateController(Mock<IUserService> serviceMock)
    {
        var logger = Mock.Of<ILogger<UsersController>>();
        return new UsersController(serviceMock.Object, logger)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext()
            }
        };
    }

    [Fact]
    public async Task Create_Should_ReturnBadRequest_When_EmailInvalid()
    {
        var service = new Mock<IUserService>();
        var controller = CreateController(service);
        controller.ModelState.AddModelError("Email", "The Email field is not a valid e-mail address.");

        var result = await controller.Create(new CreateUserDto { Email = "123#gmail.com" });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Create_Should_ReturnConflict_When_EmailExists()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync((false, "Email already exists", (UserResponseDto?)null));

        var controller = CreateController(service);

        var result = await controller.Create(new CreateUserDto { Email = "exists@example.com" });

        result.Should().BeOfType<ConflictObjectResult>();
    }

    [Fact]
    public async Task Create_Should_ReturnCreated_When_Success()
    {
        var created = new UserResponseDto { UserId = Guid.NewGuid(), Email = "new@example.com", Balance = 0m };
        var service = new Mock<IUserService>();
        service.Setup(s => s.CreateUserAsync(It.IsAny<CreateUserDto>()))
            .ReturnsAsync((true, (string?)null, created));

        var controller = CreateController(service);

        var result = await controller.Create(new CreateUserDto { Email = "new@example.com" });

        result.Should().BeOfType<CreatedAtActionResult>();
        var typed = result as CreatedAtActionResult;
        typed!.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public async Task GetBalance_Should_ReturnNotFound_When_UserMissing()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.GetBalanceAsync(It.IsAny<Guid>()))
            .ReturnsAsync((UserBalanceDto?)null);
        var controller = CreateController(service);

        var result = await controller.GetBalance(Guid.NewGuid());

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task GetBalance_Should_ReturnOk_WithBalance()
    {
        var balance = new UserBalanceDto { UserId = Guid.NewGuid(), Balance = 10m };
        var service = new Mock<IUserService>();
        service.Setup(s => s.GetBalanceAsync(balance.UserId)).ReturnsAsync(balance);
        var controller = CreateController(service);

        var result = await controller.GetBalance(balance.UserId);

        result.Should().BeOfType<OkObjectResult>();
        (result as OkObjectResult)!.Value.Should().BeEquivalentTo(balance);
    }

    [Fact]
    public async Task Deposit_Should_ReturnNotFound_When_UserMissing()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.DepositAsync(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync((UserBalanceDto?)null);
        var controller = CreateController(service);

        var result = await controller.Deposit(Guid.NewGuid(), new DepositDto { Amount = 1m });

        result.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Deposit_Should_ReturnOk_WithNewBalance()
    {
        var newBal = new UserBalanceDto { UserId = Guid.NewGuid(), Balance = 7m };
        var service = new Mock<IUserService>();
        service.Setup(s => s.DepositAsync(newBal.UserId, 2m)).ReturnsAsync(newBal);
        var controller = CreateController(service);

        var result = await controller.Deposit(newBal.UserId, new DepositDto { Amount = 2m });

        result.Should().BeOfType<OkObjectResult>();
        var payload = (result as OkObjectResult)!.Value;
        payload.Should().BeEquivalentTo(new { userId = newBal.UserId, newBalance = newBal.Balance });
    }

    [Fact]
    public async Task Deposit_Should_ReturnBadRequest_When_AmountIsNonNumericString()
    {
        var service = new Mock<IUserService>();
        var controller = CreateController(service);
        controller.ModelState.AddModelError("Amount", "The value '$' is not valid.");

        var result = await controller.Deposit(Guid.NewGuid(), new DepositDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Deposit_Should_ReturnBadRequest_When_AmountHasComma()
    {
        var service = new Mock<IUserService>();
        var controller = CreateController(service);
        controller.ModelState.AddModelError("Amount", "The value '0,33' is not valid.");

        var result = await controller.Deposit(Guid.NewGuid(), new DepositDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Withdraw_Should_ReturnNotFound_When_UserMissing()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.WithdrawAsync(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync((false, "User not found", (UserBalanceDto?)null));
        var controller = CreateController(service);

        var result = await controller.Withdraw(Guid.NewGuid(), new WithdrawDto { Amount = 1m });

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Withdraw_Should_ReturnBadRequest_When_Insufficient()
    {
        var service = new Mock<IUserService>();
        service.Setup(s => s.WithdrawAsync(It.IsAny<Guid>(), It.IsAny<decimal>()))
            .ReturnsAsync((false, "Insufficient funds", (UserBalanceDto?)null));
        var controller = CreateController(service);

        var result = await controller.Withdraw(Guid.NewGuid(), new WithdrawDto { Amount = 10m });

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Withdraw_Should_ReturnOk_When_Success()
    {
        var res = new UserBalanceDto { UserId = Guid.NewGuid(), Balance = 3m };
        var service = new Mock<IUserService>();
        service.Setup(s => s.WithdrawAsync(res.UserId, 2m))
            .ReturnsAsync((true, (string?)null, res));
        var controller = CreateController(service);

        var result = await controller.Withdraw(res.UserId, new WithdrawDto { Amount = 2m });

        result.Should().BeOfType<OkObjectResult>();
        var payload = (result as OkObjectResult)!.Value;
        payload.Should().BeEquivalentTo(new { userId = res.UserId, newBalance = res.Balance });
    }

    [Fact]
    public async Task Withdraw_Should_ReturnBadRequest_When_AmountIsNonNumericString()
    {
        var service = new Mock<IUserService>();
        var controller = CreateController(service);
        controller.ModelState.AddModelError("Amount", "The value '$' is not valid.");

        var result = await controller.Withdraw(Guid.NewGuid(), new WithdrawDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Withdraw_Should_ReturnBadRequest_When_AmountHasComma()
    {
        var service = new Mock<IUserService>();
        var controller = CreateController(service);
        controller.ModelState.AddModelError("Amount", "The value '0,33' is not valid.");

        var result = await controller.Withdraw(Guid.NewGuid(), new WithdrawDto());

        result.Should().BeOfType<BadRequestObjectResult>();
    }
}


