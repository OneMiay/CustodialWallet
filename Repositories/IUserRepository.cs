using CustodialWallet.Models;

namespace CustodialWallet.Repositories
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user, CancellationToken ct = default);
        Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
        Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
        Task UpdateAsync(User user, CancellationToken ct = default);
    }
}
