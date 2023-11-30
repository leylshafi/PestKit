using System.ComponentModel.DataAnnotations;

namespace PestKit.Areas.Admin.ViewModels
{
    public class CreatePositionVM
    {
        [Required]
        public string Name { get; set; }
    }
}
