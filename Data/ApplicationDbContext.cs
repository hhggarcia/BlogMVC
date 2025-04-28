using BlogMVC.Entity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BlogMVC.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions options) : base(options)
        {
            
        }

        protected ApplicationDbContext()
        {
            
        }

        public DbSet<Entry> Entries { get; set; }
        public DbSet<Comment> Comments { get; set; }
    }
}
