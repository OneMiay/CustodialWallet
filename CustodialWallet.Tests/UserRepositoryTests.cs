using CustodialWallet.Data;
using CustodialWallet.Models;
using CustodialWallet.Repositories;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CustodialWallet.Tests;

public class UserRepositoryTests
{
    private static AppDbContext CreateInMemoryDb(string name)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: name)
            .Options;
        return new AppDbContext(options);
    }

    [Fact]
    public async Task Create_And_Get_Should_Work()
    {
        using var db = CreateInMemoryDb(Guid.NewGuid().ToString());
        var repo = new UserRepository(db);

        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", Balance = 0m };
        await repo.CreateAsync(user);

        var fetchedById = await repo.GetByIdAsync(user.Id);
        var fetchedByEmail = await repo.GetByEmailAsync(user.Email);

        fetchedById.Should().NotBeNull();
        fetchedByEmail.Should().NotBeNull();
        fetchedById!.Email.Should().Be("a@b.com");
        fetchedByEmail!.Id.Should().Be(user.Id);
    }

    [Fact]
    public async Task Update_Should_Persist_Changes()
    {
        using var db = CreateInMemoryDb(Guid.NewGuid().ToString());
        var repo = new UserRepository(db);

        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", Balance = 1m };
        await repo.CreateAsync(user);

        user.Balance = 5m;
        await repo.UpdateAsync(user);

        (await repo.GetByIdAsync(user.Id))!.Balance.Should().Be(5m);
    }
}




