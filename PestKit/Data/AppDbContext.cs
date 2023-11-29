using Microsoft.EntityFrameworkCore;
using PestKit.Models;

namespace PestKit.Data
{
    public class AppDbContext:DbContext
    {
        public AppDbContext(DbContextOptions options):base(options) { }

        public DbSet<Blog> Blogs{ get; set; }
        public DbSet<Author> Authors{ get; set; }
        public DbSet<Tag> Tags{ get; set; }
        public DbSet<BlogTag> BlogTags { get; set; }
        public DbSet<Project> Projects { get; set; }
        public DbSet<ProjectImage> ProjectImages { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Position> Positions { get; set; }
    }
}
