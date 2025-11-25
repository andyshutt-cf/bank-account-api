using System.ComponentModel.DataAnnotations;

namespace BankAccountAPI.Models
{
    public class TransferRequest
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "FromAccountId must be a positive number")]
        public int FromAccountId { get; set; }
        
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "ToAccountId must be a positive number")]
        public int ToAccountId { get; set; }
        
        [Required]
        [Range(typeof(decimal), "0.01", "79228162514264337593543950335", ErrorMessage = "Amount must be greater than zero")]
        public decimal Amount { get; set; }
    }
}
