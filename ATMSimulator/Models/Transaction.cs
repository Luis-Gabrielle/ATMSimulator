using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ATMSimulator.Models
{
    public class Transaction
    {
        [Key]
        public int TransactionID { get; set; }

        [ForeignKey("Account")]
        public int AccountID { get; set; }

        [Required]
        public string TransactionType { get; set; } // "Deposit", "Withdraw", "Transfer"

        [Required]
        public decimal Amount { get; set; }

        public DateTime TransactionDate { get; set; } = DateTime.Now;

        public Account Account { get; set; }
    }
}
