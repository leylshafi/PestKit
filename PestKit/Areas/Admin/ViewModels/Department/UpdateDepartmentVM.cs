namespace PestKit.Areas.Admin.ViewModels
{
    public class UpdateDepartmentVM
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Photo { get; set; }
    }
}
