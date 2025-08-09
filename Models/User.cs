using System.ComponentModel.DataAnnotations;

namespace CustodialWallet.Models
{
    public class User
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        public decimal Balance { get; set; }
    }
}
