namespace CustodialWallet.DTOs
{
    public class UserResponseDto
    {
        public Guid UserId { get; set; }
        public string Email { get; set; } = null!;
        public decimal Balance { get; set; }
    }
}
