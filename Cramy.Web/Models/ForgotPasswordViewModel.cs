using System.ComponentModel.DataAnnotations;

namespace Cramy.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required(ErrorMessage = "E-posta zorunludur.")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta girin.")]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = default!;
    }
}