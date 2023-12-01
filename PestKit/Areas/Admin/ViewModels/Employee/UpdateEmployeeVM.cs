using PestKit.Models;

namespace PestKit.Areas.Admin.ViewModels
{
    public class UpdateEmployeeVM
    {
        public string Name { get; set; }
        public string? ImageUrl { get; set; }
        public IFormFile? Image { get; set; }
        public string? InstaLink { get; set; }
        public string? FbLink { get; set; }
        public string? TwitterLink { get; set; }
        public string? LinkedinLink { get; set; }
        public int DepartmentId { get; set; }
        public List<Department>? Departments { get; set; }
        public int PositionId { get; set; }
        public List<Position>? Positions { get; set; }
    }
}
