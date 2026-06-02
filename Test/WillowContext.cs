using MassTransit;
using Microsoft.EntityFrameworkCore;
using Test.Module.Entities;

namespace Test;

public class WillowContext : DbContext
{
    public WillowContext(DbContextOptions<WillowContext> options) : base(options)
    {
        
    }
    
    public DbSet<WillowEntity> WillowEntities { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        modelBuilder.AddInboxStateEntity();
        modelBuilder.AddOutboxMessageEntity();
        modelBuilder.AddOutboxStateEntity();

        modelBuilder.ApplyConfiguration<ModuleEntity>(new ModuleEntityConfiguration());
    }
}