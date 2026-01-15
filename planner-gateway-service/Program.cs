using Microsoft.AspNetCore.WebSockets;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxConcurrentConnections = 200;
    serverOptions.Limits.MaxConcurrentUpgradedConnections = 200;
});

var configurationBuilder = new ConfigurationBuilder();
configurationBuilder
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile("ocelot.routes.json", optional: false, reloadOnChange: true)
    .AddEnvironmentVariables();


builder.Services.AddOcelot(configurationBuilder.Build());
builder.Services.AddWebSockets(options =>
{
    options.KeepAliveInterval = TimeSpan.FromSeconds(120);
});
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins", builder =>
    {
        builder.AllowAnyOrigin()
        .AllowAnyMethod()
        .AllowAnyHeader();
    });
});
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Error);

var app = builder.Build();

app.UseHttpsRedirection();
app.UseCors("AllowAllOrigins");

app.UseRouting();
app.UseAuthorization();
app.UseWebSockets();

app.UseOcelot().Wait();

app.MapGet("/", () => "Gateway works!");

app.Run();