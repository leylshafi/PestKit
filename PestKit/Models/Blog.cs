namespace PestKit.Models
{
    public class Blog
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime  CreatedDate{ get; set; }
        public int AuthorId { get; set; }
		public string? ImageUrl { get; set; }
		public Author? Author { get; set; }
        public List<BlogTag> BlogTags { get; set; }
    }
}
