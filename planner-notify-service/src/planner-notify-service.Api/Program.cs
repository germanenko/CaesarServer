using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using planner_notify_service.App.Service;
using planner_notify_service.Core.IService;
using planner_notify_service.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);
var app = builder.Build();
app = ConfigureApplication(app);
app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
void ConfigureServices(IServiceCollection services)
{
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

    services.AddAuthorization();

    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IMainMonitoringService, MainMonitoringService>();
    services.AddSingleton<INotificationService, NotificationService>();
    services.AddSingleton<IMainMonitoringConnector, MainMonitoringConnector>();
    services.AddSingleton<INotificationConnector, NotificationConnector>();

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