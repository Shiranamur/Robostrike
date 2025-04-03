using Microsoft.EntityFrameworkCore;

namespace BlazorApp1.Models
{
    public class RobostrikeContext : DbContext
    {
        public RobostrikeContext(DbContextOptions<RobostrikeContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
        }
    }
}
