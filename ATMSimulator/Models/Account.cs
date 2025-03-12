using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATMSimulator.Models
{
    public class Account
    {
        [Key]
        public int AccountID { get; set; }

        [ForeignKey("User")]
        public int UserID { get; set; }

        [Required]
        public string AccountType { get; set; } // "Savings" or "Current"

        [Required]
        public decimal Balance { get; set; }

        public User User { get; set; }
    }
}
