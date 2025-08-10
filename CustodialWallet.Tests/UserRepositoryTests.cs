using CustodialWallet.Models;
using CustodialWallet.Repositories;
using Dapper;
using FluentAssertions;
using Npgsql;

namespace CustodialWallet.Tests;

public class UserRepositoryTests
{
    private NpgsqlDataSource? _dataSource;

    private bool TryInitDataSource(out NpgsqlDataSource dataSource)
    {
        var cs = Environment.GetEnvironmentVariable("TEST_POSTGRES");
        if (string.IsNullOrWhiteSpace(cs))
        {
            dataSource = null!;
            return false;
        }
        dataSource = new NpgsqlDataSourceBuilder(cs).Build();
        _dataSource = dataSource;
        using var conn = dataSource.OpenConnection();
        conn.Execute(@"CREATE TABLE IF NOT EXISTS users (
            id uuid PRIMARY KEY,
            email text NOT NULL UNIQUE,
            balance numeric(38,18) NOT NULL
        );");
        return true;
    }

    private UserRepository CreateRepository()
    {
        if (_dataSource is null)
        {
            throw new InvalidOperationException("DataSource is not initialized. Set TEST_POSTGRES env var to a valid PostgreSQL connection string.");
        }
        return new UserRepository(_dataSource.CreateConnection());
    }

    [Fact]
    public async Task Create_And_Get_Should_Work()
    {
        if (!TryInitDataSource(out _)) return; // Skip if no Postgres available
        var repo = CreateRepository();

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
        if (!TryInitDataSource(out _)) return; // Skip if no Postgres available
        var repo = CreateRepository();

        var user = new User { Id = Guid.NewGuid(), Email = "a@b.com", Balance = 1m };
        await repo.CreateAsync(user);

        user.Balance = 5m;
        await repo.UpdateAsync(user);

        (await repo.GetByIdAsync(user.Id))!.Balance.Should().Be(5m);
    }
}





