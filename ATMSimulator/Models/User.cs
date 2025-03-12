using System.ComponentModel.DataAnnotations;

namespace ATMSimulator.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required, MaxLength(16)]
        public string CardNumber { get; set; }

        [Required, MaxLength(4)]
        public string PIN { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; }

        [Required, EmailAddress]
        public string Email { get; set; }

        [Required, MaxLength(2)]
        public string ExpirationMonth { get; set; }  // MM

        [Required, MaxLength(4)]
        public string ExpirationYear { get; set; }   // YYYY
    }
}
