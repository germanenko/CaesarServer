using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using planner_content_service.App.Service;
using planner_content_service.Core.IRepository;
using planner_content_service.Core.IService;
using planner_content_service.Infrastructure.Data;
using planner_content_service.Infrastructure.Repository;
using planner_content_service.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;
using System.Text;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();
app = ConfigureApplication(app);
ApplyMigrations(app);
app.MapGet("/", () => $"Content server work");
app.Run();



string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var contentDbConnectionString = GetEnvVar("CONTENT_DB_CONNECTION_STRING");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var hostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var username = GetEnvVar("RABBITMQ_USERNAME");
    var password = GetEnvVar("RABBITMQ_PASSWORD");
    var createTaskChatQueue = GetEnvVar("RABBITMQ_CREATE_TASK_CHAT_QUEUE_NAME");
    var createTaskChatResponseQueue = GetEnvVar("RABBITMQ_CREATE_TASK_CHAT_RESPONSE_QUEUE");
    var addAccountsToTaskChatsQueue = GetEnvVar("RABBITMQ_CHAT_ADD_ACCOUNTS_TO_TASK_CHATS_QUEUE_NAME");
    var createBoardExchange = GetEnvVar("RABBITMQ_CREATE_BOARD_EXCHANGE");
    var createColumnExchange = GetEnvVar("RABBITMQ_CREATE_COLUMN_EXCHANGE");
    var createTaskExchange = GetEnvVar("RABBITMQ_CREATE_TASK_EXCHANGE");
    var contentNodesExchange = GetEnvVar("RABBITMQ_CONTENT_NODES_EXCHANGE");

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

    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateIssuerSigningKey = true,
            ValidateLifetime = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience
        });

    services.AddAuthorization();

    ConfigureSwagger(services);

    services.AddDbContext<ContentDbContext>(options =>
    {
        options.UseNpgsql(contentDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
            builder.MigrationsAssembly("planner_content_service.Api");
        });
    });

    services.AddScoped<IUserService, UserService>();
    services.AddScoped<INodeService, NodeService>();
    services.AddScoped<ITaskService, TaskService>();
    services.AddScoped<IBoardService, BoardService>();
    services.AddScoped<IAccessService, AccessService>();

    services.AddScoped<IBoardRepository, BoardRepository>();
    services.AddScoped<IPublicationStatusRepository, PublicationStatusRepository>();
    services.AddScoped<ITaskRepository, TaskRepository>();
    services.AddScoped<INodeRepository, NodeRepository>();
    services.AddScoped<IAccessRepository, AccessRepository>();

    services.AddSingleton<IJwtService, JwtService>();

    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp =>
        new RabbitMqNotifyService(
            hostname,
            username,
            password,
            createTaskChatQueue,
            addAccountsToTaskChatsQueue,
            createBoardExchange,
            createColumnExchange,
            createTaskExchange
        ));

    services.AddHostedService(sp => new RabbitMqService
    (
        sp.GetRequiredService<IServiceScopeFactory>(),
        sp.GetRequiredService<INotifyService>(),
        hostname,
        username,
        password,
        createTaskChatResponseQueue,
        contentNodesExchange
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
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.MapGet("/", () => "Content server work");

    return app;
}

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ContentDbContext>();

        var appliedMigrations = context.Database.GetAppliedMigrations();

        var allMigrations = context.Database.GetMigrations();

        if (context.Database.GetPendingMigrations().Any())
        {
            context.Database.Migrate();
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating the database.");
    }
}

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Planner Task Board Api",
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