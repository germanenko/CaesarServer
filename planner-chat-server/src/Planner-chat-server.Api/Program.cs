using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.AspNetCore.WebSockets;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Planner_chat_server.App.Service;
using Planner_chat_server.Core.IRepository;
using Planner_chat_server.Core.IService;
using Planner_chat_server.Infrastructure.Data;
using Planner_chat_server.Infrastructure.Repository;
using Planner_chat_server.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

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

    var chatImageQueue = GetEnvVar("RABBITMQ_CHAT_IMAGE_QUEUE_NAME");
    var chatAttachmentQueue = GetEnvVar("RABBITMQ_CHAT_ATTACHMENT_QUEUE_NAME");
    var createChatQueue = GetEnvVar("RABBITMQ_CREATE_CHAT_QUEUE_NAME");
    var addAccountsToTaskChatsQueue = GetEnvVar("RABBITMQ_CHAT_ADD_ACCOUNTS_TO_TASK_CHATS_QUEUE_NAME");
    var initChatQueue = GetEnvVar("RABBITMQ_INIT_CHAT_QUEUE_NAME");
    var createTaskChatQueue = GetEnvVar("RABBITMQ_CREATE_TASK_CHAT_QUEUE_NAME");
    var createTaskChatResponseQueue = GetEnvVar("RABBITMQ_CREATE_TASK_CHAT_RESPONSE_QUEUE");
    var messageSentToChatQueue = GetEnvVar("RABBITMQ_MESSAGE_SENT_TO_CHAT_QUEUE");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var connectionString = GetEnvVar("CHAT_DB_CONNECTION_STRING");
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

    services.AddDbContext<ChatDbContext>(options =>
    {
        options.UseNpgsql(connectionString);
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

    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp =>
        new RabbitMqNotifyService(
            rabbitMqHostname,
            rabbitMqUsername,
            rabbitMqPassword,
            createChatQueue,
            createTaskChatResponseQueue,
            messageSentToChatQueue
        ));

    services.AddScoped<IChatRepository, ChatRepository>();
    services.AddScoped<IChatService, ChatService>();
    services.AddScoped<IChatConnector, ChatConnector>();

    services.AddHostedService(e => new RabbitMqService(
        e.GetRequiredService<IServiceScopeFactory>(),
        e.GetRequiredService<INotifyService>(),
        e.GetRequiredService<IChatConnectionService>(),
        rabbitMqHostname,
        rabbitMqUsername,
        rabbitMqPassword,
        initChatQueue,
        chatAttachmentQueue,
        chatImageQueue,
        addAccountsToTaskChatsQueue,
        createTaskChatQueue
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