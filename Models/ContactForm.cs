using System.ComponentModel.DataAnnotations;

namespace BrettGravesPortfolio.Models
{
    public class ContactForm
    {
        [Required, StringLength(120)]
        public string Name { get; set; } = "";

        [Required, EmailAddress, StringLength(200)]
        public string Email { get; set; } = "";

        [Required, StringLength(200)]
        public string Subject { get; set; } = "";

        [Required, StringLength(4000)]
        public string Message { get; set; } = "";

        // Honeypot (bots fill this; humans leave empty)
        public string? Website { get; set; }
    }
}