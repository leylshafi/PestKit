namespace PestKit.ViewModels
{
	public class ProfileVM
	{
        public IFormFile? ProfilePhoto { get; set; }
        public string? ProfileImage { get; set; }
        public string Name { get; set; }	
		public string Surname { get; set; }	
		public string? Bio { get; set; }
		public DateTime? Birthday { get; set;}
	}
}
