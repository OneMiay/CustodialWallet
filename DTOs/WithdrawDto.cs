using System.ComponentModel.DataAnnotations;

namespace CustodialWallet.DTOs
{
    public class WithdrawDto
    {
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
}
