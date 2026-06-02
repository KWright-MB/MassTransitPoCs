using BaseContextPoC;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.None);
builder.Logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.Warning);


Action<DbContextOptionsBuilder> configureDbContext = o =>
{
    o.UseSqlServer("Server=PA75313003M-MBX;Database=BaseContextPoC;Trusted_Connection=True;TrustServerCertificate=True;");
};

builder.Services.AddMassTransit(x =>
{
    x.ConfigureHealthCheckOptions(options =>
    {
        options.MinimalFailureStatus = HealthStatus.Healthy;
    });

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("PoC"));

    x.AddEntityFrameworkOutbox<MassTransitOutboxingContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.DisableInboxCleanupService();
        o.UseSqlServer();
        o.UseBusOutbox();
    });
    
    x.AddConsumer<SimpleMessageOneConsumer>();
    x.AddConsumer<SimpleMessageTwoConsumer>();
    
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddDbContextFactory<SimpleContextOne>(configureDbContext);
builder.Services.AddDbContext<SimpleContextOne>(configureDbContext);

builder.Services.AddDbContextFactory<SimpleContextTwo>(configureDbContext);
builder.Services.AddDbContext<SimpleContextTwo>(configureDbContext);

builder.Services.AddScoped<MassTransitOutboxingContext>(sp =>
{
    // MassTransit needs to resolve MassTransitOutboxingContext to the context being used in the current scope.
    // In our Worker, we explicitly resolve either SimpleContextOne or SimpleContextTwo.
    
    // We can check which one has been instantiated in this scope.
    // However, if neither has been instantiated yet, we need a default or a way to choose.
    
    // Using GetService doesn't instantiate. 
    // But DbContexts are registered as Scoped, so they are unique per scope.
    
    // Let's use a simpler approach: Register it for both but in a way that MT can find the one it needs.
    // Actually, MT resolves the specific type we passed to AddEntityFrameworkOutbox<T>.
    
    // If we want both to work, we can use a custom scope property or just check which one was requested.
    
    var one = sp.GetService<SimpleContextOne>();
    if (one != null) return one;

    var two = sp.GetService<SimpleContextTwo>();
    if (two != null) return two;

    // Default to One if neither is found (e.g. for the background delivery service)
    return sp.GetRequiredService<SimpleContextOne>();
});

builder.Services.AddHostedService<Worker>();

IHost host = builder.Build();
await host.RunAsync();


