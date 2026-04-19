namespace WebApplication1.Model.Entity;

public class UserContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public UserContext(DbContextOptions<UserContext> options) : base(options) { }

    public DbSet<Currency> Валюты { get; set; }
    public DbSet<Exchange_rates> КурсыВалют { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exchange_rates>()
            .HasIndex(k => new { k.Дата, k.ID_валюты })
            .IsUnique();
    }
}