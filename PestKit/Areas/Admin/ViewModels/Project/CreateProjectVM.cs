using PestKit.Models;

namespace PestKit.Areas.Admin.ViewModels
{
    public class CreateProjectVM
    {
        public string Name { get; set; }
        public IFormFile MainPhoto { get; set; }
        public List<IFormFile> Photos { get; set; }
    }
}
