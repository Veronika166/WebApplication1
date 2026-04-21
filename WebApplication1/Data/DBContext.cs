namespace WebApplication1.Data;

public class DBContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DBContext(DbContextOptions<DBContext> options) : base(options) { }

    public DbSet<Currency> Валюты { get; set; }
    public DbSet<Exchange_rates> КурсыВалют { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Exchange_rates>()
            .HasIndex(k => new { k.Дата, k.ID_валюты })
            .IsUnique();
    }
}