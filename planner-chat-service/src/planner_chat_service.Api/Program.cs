using DotNetEnv;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Console;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using planner_chat_service.App.Service;
using planner_chat_service.Core.IRepository;
using planner_chat_service.Core.IService;
using planner_chat_service.Infrastructure.Data;
using planner_chat_service.Infrastructure.Repository;
using planner_chat_service.Infrastructure.Service;
using planner_content_service.App.Service;
using planner_content_service.Infrastructure.Repository;
using planner_server_package.Events.Enums;
using planner_server_package.RabbitMQ;
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
    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUsername = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var createChatQueue = GetEnvVar("RABBITMQ_CREATE_CHAT_QUEUE_NAME");
    var addAccountsToTaskChatsQueue = GetEnvVar("RABBITMQ_CHAT_ADD_ACCOUNTS_TO_TASK_CHATS_QUEUE_NAME");
    var messageSentToChatQueue = GetEnvVar("RABBITMQ_MESSAGE_SENT_TO_CHAT_QUEUE");
    var createPersonalChatQueue = GetEnvVar("RABBITMQ_CREATE_PERSONAL_CHAT_QUEUE");
    var chatNodesExchange = GetEnvVar("RABBITMQ_CHAT_NODES_EXCHANGE");
    var getUsersWithEnabledNotifications = GetEnvVar("RABBITMQ_GET_NOTIFICATION_SETTINGS_WITH_ENABLED_EXCHANGE");
    var checkAccessExchange = GetEnvVar("RABBITMQ_CHECK_ACCESS_EXCHANGE");
    var sendNotificationExchange = GetEnvVar("RABBITMQ_SEND_NOTIFICATION");
    var getGoogleTokenExchange = GetEnvVar("RABBITMQ_GET_GOOGLE_TOKEN");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var chatConnectionString = GetEnvVar("CHAT_DB_CONNECTION_STRING");
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

    services.AddDbContext<ChatDbContext>(options =>
    {
        options.UseNpgsql(chatConnectionString, sp => sp.MigrationsAssembly("planner_chat_service.Api"));
    });

    services.AddWebSockets(options =>
    {
        options.KeepAliveInterval = TimeSpan.FromSeconds(120);
    });
    ConfigureJwtAuthentication(services, jwtSecret, jwtIssuer, jwtAudience);
    ConfigureSwagger(services);

    services.AddAuthorization();

    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IChatConnectionService, ChatConnectionService>();
    services.AddScoped<IUserService, UserService>();



    services.AddScoped<IChatRepository, ChatRepository>();
    services.AddScoped<INodeRepository, NodeRepository>();
    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<INodeService, NodeService>();
    services.AddScoped<IChatConnector, ChatConnector>();

    services.AddSingleton<IPublisherService, RabbitMQPublisher>(sp =>
        new RabbitMQPublisher(
            rabbitMqHostname,
            rabbitMqUsername,
            rabbitMqPassword,
            new Dictionary<PublishEvent, string>() {
                { PublishEvent.AddAccountToChat, createChatQueue },
                { PublishEvent.MessageSentToChat, messageSentToChatQueue },
                { PublishEvent.CreatePersonalChat, createPersonalChatQueue },
                { PublishEvent.GetNotificationSettings, getUsersWithEnabledNotifications },
                { PublishEvent.CheckAccess, checkAccessExchange },
                { PublishEvent.SendNotification, sendNotificationExchange },
                { PublishEvent.GetGoogleToken, getGoogleTokenExchange },
            },
            sp.GetRequiredService<ILogger<RabbitMQPublisher>>()
        ));

    services.AddHostedService<GmailReaderService>();
    services.AddHostedService(e => new RabbitMqService(
        e.GetRequiredService<IServiceScopeFactory>(),
        rabbitMqHostname,
        rabbitMqUsername,
        rabbitMqPassword,
        "_chat",
        e.GetRequiredService<IPublisherService>(),
        e.GetRequiredService<ILogger<RabbitMqService>>(),
        addAccountsToTaskChatsQueue,
        chatNodesExchange
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
    app.MapGet("/", () => "Chat server work");

    return app;
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Planer chat api",
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

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ChatDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
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