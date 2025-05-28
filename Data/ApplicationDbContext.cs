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

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<Comment>().HasQueryFilter(c => !c.Borrado);
            builder.Entity<Entry>().HasQueryFilter(c => !c.Borrado);
        }

        public DbSet<Entry> Entries { get; set; }
        public DbSet<Comment> Comments { get; set; }
        public DbSet<Lote> Lotes { get; set; }
    }
}
