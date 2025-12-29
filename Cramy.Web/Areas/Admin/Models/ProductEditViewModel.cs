using System.ComponentModel.DataAnnotations;

namespace Cramy.Web.Areas.Admin.Models
{
    public class ProductEditViewModel : ProductCreateViewModel
    {
        [Required]
        public Guid Id { get; set; }
    }
}
