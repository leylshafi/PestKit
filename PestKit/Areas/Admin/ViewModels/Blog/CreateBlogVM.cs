
using PestKit.Models;
using System.ComponentModel.DataAnnotations;

namespace PestKit.Areas.Admin.ViewModels
{
	public class CreateBlogVM
	{
		[Required]
		[MaxLength(25,ErrorMessage ="Can not exceed 25")]
		public string Name { get; set; }
		public string? Description { get; set; }
		public int AuthorId { get; set; }
		[Required]
		public IFormFile? Photo { get; set; }
        public List<int>? TagIds { get; set; }
		public List<Tag>? Tags { get; set; }
        public List<Author>? Authors { get; set; }
    }
}
