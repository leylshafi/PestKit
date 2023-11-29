using PestKit.Models;

namespace PestKit.Areas.Admin.ViewModels
{
    public class CreateDepartmentVM
    {
        public string Name { get; set; }
        public IFormFile Photo { get; set; }
    }
}
