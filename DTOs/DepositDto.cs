using System.ComponentModel.DataAnnotations;

namespace CustodialWallet.DTOs
{
    public class DepositDto
    {
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
}
