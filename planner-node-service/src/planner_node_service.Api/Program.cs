using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using planner_node_service.App.Service;
using planner_node_service.Core;
using planner_node_service.Core.IRepository;
using planner_node_service.Core.IService;
using planner_node_service.Infrastructure.Data;
using planner_node_service.Infrastructure.Repository;
using planner_node_service.Infrastructure.Service;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

Env.Load();

var builder = WebApplication.CreateBuilder(args);

ConfigureServices(builder.Services);
var app = builder.Build();
app = ConfigureApplication(app);

ApplyMigrations(app);

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");
void ConfigureServices(IServiceCollection services)
{
    var nodeDbConnectionString = GetEnvVar("NODE_DB_CONNECTION_STRING");

    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUsername = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var messageSentToChatQueue = GetEnvVar("RABBITMQ_MESSAGE_SENT_TO_CHAT_QUEUE");
    var createPersonalChatQueue = GetEnvVar("RABBITMQ_CREATE_PERSONAL_CHAT_QUEUE");
    var createBoardExchange = GetEnvVar("RABBITMQ_CREATE_BOARD_EXCHANGE");
    var createColumnExchange = GetEnvVar("RABBITMQ_CREATE_COLUMN_EXCHANGE");
    var createTaskExchange = GetEnvVar("RABBITMQ_CREATE_TASK_EXCHANGE");
    var contentNodesExchange = GetEnvVar("RABBITMQ_CONTENT_NODES_EXCHANGE");
    var chatNodesExchange = GetEnvVar("RABBITMQ_CHAT_NODES_EXCHANGE");
    var getUsersWithEnabledNotifications = GetEnvVar("RABBITMQ_GET_NOTIFICATION_SETTINGS_WITH_ENABLED_EXCHANGE");
    var checkAccessExchange = GetEnvVar("RABBITMQ_CHECK_ACCESS_EXCHANGE");
    var deleteNodeExchange = GetEnvVar("RABBITMQ_DELETE_NODE_EXCHANGE");
    var scopeUpdatedExchange = GetEnvVar("RABBITMQ_SCOPE_UPDATED_EXCHANGE");

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

    services.AddDbContext<INodeDbContext, NodeDbContext>(options =>
    {
        options.UseNpgsql(nodeDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            builder.MigrationsAssembly("planner_node_service.Api");
        });
    });


    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IWebSocketService, WebSocketService>();

    services.AddSingleton<IPublisherService, RabbitMQPublisher>(sp =>
        new RabbitMQPublisher(
            rabbitMqHostname,
            rabbitMqUsername,
            rabbitMqPassword,
            new Dictionary<PublishEvent, string> {
                { PublishEvent.ContentNodes, contentNodesExchange },
                { PublishEvent.ChatNodes, chatNodesExchange },
                { PublishEvent.ScopeUpdated, scopeUpdatedExchange },
            },
            sp.GetRequiredService<ILogger<RabbitMQPublisher>>()
        ));

    services.AddScoped<INodeService, NodeService>();
    services.AddScoped<IAccessService, AccessService>();
    services.AddScoped<INotificationService, NotificationService>();
    services.AddScoped<IScopeRepository, ScopeRepository>();
    services.AddScoped<INodeRepository, NodeRepository>();
    services.AddScoped<IAccessRepository, AccessRepository>();
    services.AddScoped<ILogRepository, LogRepository>();
    services.AddScoped<INotificationRepository, NotificationRepository>();

    services.AddHostedService(sp => new RabbitMqService(
        sp.GetRequiredService<IServiceScopeFactory>(),
        rabbitMqHostname,
        rabbitMqUsername,
        rabbitMqPassword,
        "_node",
        sp.GetRequiredService<IPublisherService>(),
        sp.GetRequiredService<ILogger<RabbitMqService>>(),
        sp.GetRequiredService<IWebSocketService>(),
        messageSentToChatQueue,
        createPersonalChatQueue,
        createBoardExchange,
        createColumnExchange,
        createTaskExchange,
        getUsersWithEnabledNotifications,
        checkAccessExchange,
        deleteNodeExchange
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

    app.MapGet("/", () => "Node service work");

    return app;
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Planer node service api",
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
        var context = services.GetRequiredService<NodeDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}