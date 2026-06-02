using Microsoft.EntityFrameworkCore;

namespace BaseContextPoC;

public sealed class SimpleContextTwo : MassTransitOutboxingContext
{
    public SimpleContextTwo(DbContextOptions<SimpleContextTwo> options) : base(options, excludeOutboxFromMigrations: true)
    {
    }
    
    public DbSet<SimpleEntityTwo> SimpleEntityTwo { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema("Custom");
        base.OnModelCreating(modelBuilder);
    }
}