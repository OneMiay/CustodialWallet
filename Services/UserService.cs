using CustodialWallet.DTOs;
using CustodialWallet.Models;
using CustodialWallet.Repositories;

namespace CustodialWallet.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly ILogger<UserService> _logger;

        public UserService(IUserRepository repo, ILogger<UserService> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        public async Task<(bool Success, string? Error, UserResponseDto? Result)> CreateUserAsync(CreateUserDto dto)
        {
            // Check if email already exists
            var existing = await _repo.GetByEmailAsync(dto.Email);
            if (existing != null)
            {
                _logger.LogWarning("Attempt to create user with existing emil {Email}", dto.Email);
                return (false, "Email already exists", null);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = dto.Email,
                Balance = 0m
            };

            await _repo.CreateAsync(user);
            _logger.LogInformation("Created user {UserId}", user.Id);

            return (true, null, new UserResponseDto { UserId = user.Id, Email = user.Email, Balance = user.Balance });
        }

        public async Task<UserBalanceDto?> GetBalanceAsync(Guid userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return null;
            return new UserBalanceDto { UserId = user.Id, Balance = user.Balance };
        }

        public async Task<UserBalanceDto?> DepositAsync(Guid userId, decimal amount)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return null;

            user.Balance += amount;
            await _repo.UpdateAsync(user);
            _logger.LogInformation("Deposit {Amount} to {UserId}. New balance: {Balance}", amount, userId, user.Balance);

            return new UserBalanceDto { UserId = user.Id, Balance = user.Balance };
        }

        public async Task<(bool Success, string? Error, UserBalanceDto? Result)> WithdrawAsync(Guid userId, decimal amount)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return (false, "User not found", null);

            if (user.Balance < amount)
            {
                _logger.LogWarning("Withdraw failed for {UserId}: insufficient funds. Requested {Requested}, Available {Available}", userId, amount, user.Balance);
                return (false, "Insufficient funds", null);
            }

            user.Balance -= amount;
            await _repo.UpdateAsync(user);
            _logger.LogInformation("Withdraw {Amount} from {UserId}. New balance: {Balance}", amount, userId, user.Balance);

            return (true, null, new UserBalanceDto { UserId = user.Id, Balance = user.Balance });
        }
    }
}
