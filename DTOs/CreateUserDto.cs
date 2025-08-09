using System.ComponentModel.DataAnnotations;

namespace CustodialWallet.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;
    }
}
