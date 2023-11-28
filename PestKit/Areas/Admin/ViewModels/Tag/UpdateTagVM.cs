using System.ComponentModel.DataAnnotations;

namespace PestKit.Areas.Admin.ViewModels
{
    public class UpdateTagVM
    {
        [Required(ErrorMessage = "Name is required")]
        [MaxLength(25, ErrorMessage = "Name's max length is 25")]
        public string Name { get; set; }
    }
}
