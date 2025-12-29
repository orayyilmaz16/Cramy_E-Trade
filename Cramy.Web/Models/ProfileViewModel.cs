using System.ComponentModel.DataAnnotations;

namespace Cramy.Web.Models
{
    public class ProfileViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;

        [Display(Name = "Telefon")]
        [Phone]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Ad")]
        public string? FirstName { get; set; }

        [Display(Name = "Soyad")]
        public string? LastName { get; set; }
    }
}
