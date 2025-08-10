using CustodialWallet.Models;
using Dapper;
using Npgsql;
using System.Data;

namespace CustodialWallet.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly IDbConnection _connection;

        public UserRepository(IDbConnection connection)
        {
            _connection = connection;
        }

        public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        {
            const string sql = @"INSERT INTO users (id, email, balance) VALUES (@Id, @Email, @Balance);";
            await _connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: ct));
            return user;        
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            const string sql = @"SELECT id, email, balance FROM users WHERE id = @Id LIMIT 1;";
            return _connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            const string sql = @"SELECT id, email, balance FROM users WHERE email = @Email LIMIT 1;";
            return _connection.QueryFirstOrDefaultAsync<User>(new CommandDefinition(sql, new { Email = email }, cancellationToken: ct));
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            const string sql = @"UPDATE users SET email = @Email, balance = @Balance WHERE id = @Id;";
            await _connection.ExecuteAsync(new CommandDefinition(sql, user, cancellationToken: ct));
        }
    }
}
