using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Test;
using Test.Module;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);



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
// This is required for the outbox to work, if we skip the service provider the publish endpoint didn't track the changes
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<WillowContext>());


builder.Services.AddHostedService<NonModuleWorker>();
builder.Services.AddHostedService<ModuleWorker>();

IHost host = builder.Build();


var scope = host.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetRequiredService<DbContext>();
await dbContext.Database.MigrateAsync();

await host.RunAsync();