using System.Text;
using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using planner_notify_service.App.Service;
using planner_notify_service.Core.IRepository;
using planner_notify_service.Core.IService;
using planner_notify_service.Infrastructure.Data;
using planner_notify_service.Infrastructure.Repository;
using planner_notify_service.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

Env.Load();

ConfigureServices(builder.Services);
var app = builder.Build();
app = ConfigureApplication(app);

ApplyMigrations(app);

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
void ConfigureServices(IServiceCollection services)
{
    var firebaseProjectId = GetEnvVar("FIREBASE_PROJECT_ID");
    var firebaseClientEmail = GetEnvVar("FIREBASE_CLIENT_EMAIL");
    var firebasePrivateKey = GetEnvVar("FIREBASE_PRIVATE_KEY");

    //var notifyDbConnectionString = GetEnvVar("NOTIFY_DB_CONNECTION_STRING");
    var notifyDbConnectionString = "Server=188.225.18.18:5437;Database=planner-notify;User Id=user;Password=*Planner;";

    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUsername = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var messageSentToChatQueue = GetEnvVar("RABBITMQ_MESSAGE_SENT_TO_CHAT_QUEUE");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

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

    services.AddDbContext<NotifyDbContext>(options =>
    {
        options.UseNpgsql(notifyDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            builder.MigrationsAssembly("planner-notify-service.Api");
        });
    });

    services.AddSingleton<IFirebaseService, FirebaseService>(sp =>
        new FirebaseService(
            firebaseProjectId,
            firebaseClientEmail,
            firebasePrivateKey
        ));

    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IMainMonitoringService, MainMonitoringService>();
    services.AddSingleton<INotificationService, NotificationService>();
    services.AddSingleton<IMainMonitoringConnector, MainMonitoringConnector>();
    services.AddSingleton<INotificationConnector, NotificationConnector>();

    services.AddScoped<INotifyService, NotifyService>();
    services.AddScoped<INotifyRepository, NotifyRepository>();

    services.AddHostedService<RabbitMqService>(sp => new RabbitMqService(
        sp.GetRequiredService<INotificationService>(),
        rabbitMqHostname,
        rabbitMqUsername,
        rabbitMqPassword,
        messageSentToChatQueue
    ));
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
        var context = services.GetRequiredService<NotifyDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}