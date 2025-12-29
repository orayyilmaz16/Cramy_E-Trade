using System.ComponentModel.DataAnnotations;

namespace Cramy.Web.Models
{
    public class RegisterViewModel
    {
        [Required, EmailAddress]
        public string Email { get; set; } = default!;
        [Required, DataType(DataType.Password)]
        public string Password { get; set; } = default!;
        [DataType(DataType.Password), Compare("Password")]
        public string ConfirmPassword { get; set; } = default!;
    }

}
