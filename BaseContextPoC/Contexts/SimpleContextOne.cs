using Microsoft.EntityFrameworkCore;

namespace BaseContextPoC;

public sealed class SimpleContextOne : MassTransitOutboxingContext
{
    public SimpleContextOne(DbContextOptions<SimpleContextOne> options) : base(options)
    {
    }
    
    public DbSet<SimpleEntityOne> SimpleEntityOne { get; set; }
    
}