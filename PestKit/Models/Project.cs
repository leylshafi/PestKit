namespace PestKit.Models
{
	public class Project
	{
		public int Id { get; set; }
		public string Name { get; set; }

        public List<ProjectImage> ProjectImages { get; set; }
    }
}
