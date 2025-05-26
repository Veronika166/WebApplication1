using Microsoft.EntityFrameworkCore;
using WebApplication1;
using WebApplication1.Model;

namespace WebApplication1
{
    public class UserContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public UserContext(DbContextOptions<UserContext> options) : base(options) { }

        public DbSet<Валюта> Валюты { get; set; }
        public DbSet<Курсы_валют> КурсыВалют { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Курсы_валют>()
                .HasIndex(k => new { k.Дата, k.ID_валюты })
                .IsUnique();
        }
    }
}