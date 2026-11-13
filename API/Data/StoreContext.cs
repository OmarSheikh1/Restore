using Microsoft.EntityFrameworkCore;
namespace API.Data;

public class StoreContext(DbContextOptions options) : DbContext(options)
{
    public DbSet<API.Entities.Product> Products { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
    }
}

