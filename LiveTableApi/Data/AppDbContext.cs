using LiveTableApi.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace LiveTableApi.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Genre> Genres { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    }
}
