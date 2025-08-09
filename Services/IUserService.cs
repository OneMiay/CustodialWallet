using CustodialWallet.DTOs;

namespace CustodialWallet.Services
{
    public interface IUserService
    {
        Task<(bool Success, string? Error, UserResponseDto? Result)> CreateUserAsync(CreateUserDto dto);
        Task<UserBalanceDto?> GetBalanceAsync(Guid userId);
        Task<UserBalanceDto?> DepositAsync(Guid userId, decimal amount);
        Task<(bool Success, string? Error, UserBalanceDto? Result)> WithdrawAsync(Guid userId, decimal amount);
    }
}
