using MassTransit;
using Microsoft.EntityFrameworkCore;

namespace BaseContextPoC;

public class MassTransitOutboxingContext : DbContext
{
    private readonly bool _excludeOutboxFromMigrations;

    public MassTransitOutboxingContext(DbContextOptions options) : base(options)
    {
    }

    protected MassTransitOutboxingContext(DbContextOptions options, bool excludeOutboxFromMigrations) : base(options)
    {
        _excludeOutboxFromMigrations = excludeOutboxFromMigrations;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.AddInboxStateEntity(e =>
        {
            e.ToTable("InboxState", "dbo");
            if (_excludeOutboxFromMigrations)
            {
                e.ToTable(t => t.ExcludeFromMigrations());
            }
        });
        modelBuilder.AddOutboxMessageEntity(e =>
        {
            e.ToTable("OutboxMessage", "dbo");
            if (_excludeOutboxFromMigrations)
            {
                e.ToTable(t => t.ExcludeFromMigrations());
            }
        });
        modelBuilder.AddOutboxStateEntity(e =>
        {
            e.ToTable("OutboxState", "dbo");
            if (_excludeOutboxFromMigrations)
            {
                e.ToTable(t => t.ExcludeFromMigrations());
            }
        });
    }
}