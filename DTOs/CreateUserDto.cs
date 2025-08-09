using System.ComponentModel.DataAnnotations;

namespace CustodialWallet.DTOs
{
    public class CreateUserDto
    {
        [Required]
        [EmailAddress(ErrorMessage = "Email is not valid")]
        public string Email { get; set; } = null!;
    }
}
