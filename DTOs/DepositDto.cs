using System.ComponentModel.DataAnnotations;

namespace CustodialWallet.DTOs
{
    public class DepositDto
    {
        [RegularExpression(@"^\d+(\.\d+)?$", ErrorMessage = "Amount must be a number with '.' as decimal separator")]
        [Range(0.00000001, double.MaxValue, ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
}
