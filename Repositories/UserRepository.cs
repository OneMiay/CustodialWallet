using CustodialWallet.Data;
using CustodialWallet.Models;
using Microsoft.EntityFrameworkCore;

namespace CustodialWallet.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly AppDbContext _db;

        public UserRepository(AppDbContext db) => _db = db;

        public async Task<User> CreateAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Add(user);
            await _db.SaveChangesAsync(ct);
            return user;
        }

        public Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Id == id, ct);
        }

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            return _db.Users.FirstOrDefaultAsync(u => u.Email == email, ct);
        }

        public async Task UpdateAsync(User user, CancellationToken ct = default)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync(ct);
        }
    }
}
