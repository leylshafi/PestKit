using System.ComponentModel.DataAnnotations;

namespace PestKit.Areas.Admin.ViewModels
{
	public class UpdateAuthorVM
	{
		[Required]
		[MaxLength(25, ErrorMessage = "Length can not exceed 25")]
		public string Name { get; set; }

		[Required]
		[MaxLength(25, ErrorMessage = "Length can not exceed 25")]
		public string Surname { get; set; }
	}
}
