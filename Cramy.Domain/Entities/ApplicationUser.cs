
using Microsoft.AspNetCore.Identity;

namespace Cramy.Domain.Entities
{
    public class ApplicationUser : IdentityUser
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
    }


}
