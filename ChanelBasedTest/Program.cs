using ChanelBasedTest;
using ChanelBasedTest.Module;
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
    o.UseSqlServer("Server=PA75313003M-MBX;Database=OneContextPoC;Trusted_Connection=True;TrustServerCertificate=True;");
    o.ConfigureWarnings(w => w.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
};

builder.Services.AddMassTransit(x =>
{
    x.ConfigureHealthCheckOptions(options =>
    {
        options.MinimalFailureStatus = HealthStatus.Healthy;
    });

    x.SetEndpointNameFormatter(new KebabCaseEndpointNameFormatter("PoC"));

    x.AddEntityFrameworkOutbox<WillowContext>(o =>
    {
        o.QueryDelay = TimeSpan.FromSeconds(1);
        o.DisableInboxCleanupService();
        o.UseSqlServer();
        o.UseBusOutbox();
    });
    
    x.AddConsumer<NonModuleBasedMessageConsumer>();
    x.AddConsumer<ModuleBasedMessageConsumer>();
    
    x.UsingInMemory((context, cfg) =>
    {
        cfg.ConfigureEndpoints(context);
    });
});

builder.Services.AddOptions<MassTransitHostOptions>()
    .Configure(options =>
    {
        options.WaitUntilStarted = true;
    });

builder.Services.AddDbContext<WillowContext>(configureDbContext);
builder.Services.AddScoped<DbContext, WillowContext>();


//builder.Services.AddHostedService<NonModuleWorker>();
builder.Services.AddHostedService<ModuleWorker>();

IHost host = builder.Build();


var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
await dbContext.Database.MigrateAsync();

await host.RunAsync();