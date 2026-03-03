using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using planner_analytics_service.Api;
using planner_analytics_service.App.Service;
using planner_analytics_service.Core.IRepository;
using planner_analytics_service.Core.IService;
using planner_analytics_service.Infrastructure.Data;
using planner_analytics_service.Infrastructure.Repository;
using planner_analytics_service.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);
var app = builder.Build();
app = ConfigureApplication(app);

ApplyMigrations(app);

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
void ConfigureServices(IServiceCollection services)
{
    var notifyDbConnectionString = GetEnvVar("ANALYTICS_DB_CONNECTION_STRING");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    builder.Logging.AddConsole();
    builder.Logging.AddDebug();

    builder.Logging.AddSimpleConsole(options =>
    {
        options.IncludeScopes = true;
        options.SingleLine = false;
        options.TimestampFormat = "HH:mm:ss ";
        options.ColorBehavior = LoggerColorBehavior.Enabled;
    });

    builder.Logging.SetMinimumLevel(LogLevel.Information);

    services.AddControllers(e =>
    {
        e.OutputFormatters.RemoveType<HttpNoContentOutputFormatter>();
    });

    services.AddCors(setup =>
    {
        setup.AddDefaultPolicy(options =>
        {
            options.AllowAnyHeader();
            options.WithOrigins(corsAllowedOrigins.Split(","));
            options.AllowAnyMethod();
        });
    });

    services.AddWebSockets(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(120);
    });


    ConfigureJwtAuthentication(services, jwtSecret, jwtIssuer, jwtAudience);
    ConfigureSwagger(services);

    services.AddDistributedMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(15);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    services.AddAuthorization();

    services.AddDbContext<AnalyticsDbContext>(options =>
    {
        options.UseNpgsql(notifyDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            builder.MigrationsAssembly("planner_notify_service.Api");
        });
    });


    services.AddSingleton<IJwtService, JwtService>();

    services.AddScoped<IAnalyticsService, AnalyticsService>();
    services.AddScoped<IAnalyticsRepository, AnalyticsRepository>();
}

WebApplication ConfigureApplication(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseWebSockets();
    app.UseCors();
    app.UseAuthentication();
    app.UseSession();
    app.UseAuthorization();
    app.MapControllers();

    app.MapGet("/", () => "Notify service work");

    return app;
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Planer notify service api",
            Description = "Api",
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Description = "Bearer auth scheme",
            In = ParameterLocation.Header,
            Name = "Authorization",
            Type = SecuritySchemeType.ApiKey
        });

        options.OperationFilter<SecurityRequirementsOperationFilter>();
        options.EnableAnnotations();
    });
}


void ConfigureJwtAuthentication(IServiceCollection services, string secret, string issuer, string audience)
{
    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            ValidIssuer = issuer,
            ValidAudience = audience
        });
}

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AnalyticsDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}