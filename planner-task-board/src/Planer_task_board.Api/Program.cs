using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Planer_task_board.App.Service;
using Planer_task_board.Core.IRepository;
using Planer_task_board.Core.IService;
using Planer_task_board.Infrastructure.Data;
using Planer_task_board.Infrastructure.Repository;
using Planer_task_board.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
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
    var createTaskChatQueue = GetEnvVar("RABBITMQ_CREATE_TASK_CHAT_QUEUE");
    var createTaskChatResponseQueue = GetEnvVar("RABBITMQ_CREATE_TASK_CHAT_RESPONSE_QUEUE");
    var addAccountsToTaskChatsQueue = GetEnvVar("RABBITMQ_CHAT_ADD_ACCOUNTS_TO_TASK_CHATS_QUEUE_NAME");

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
        });
    });

    services.AddScoped<ITaskService, TaskService>();
    services.AddScoped<IBoardService, BoardService>();
    services.AddScoped<IDraftService, DraftService>();

    services.AddScoped<IBoardRepository, BoardRepository>();
    services.AddScoped<IDeletedTaskRepository, DeletedTaskRepository>();
    services.AddScoped<ITaskRepository, TaskRepository>();

    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp => new RabbitMqNotifyService
    (
        hostname,
        username,
        password,
        createTaskChatQueue,
        addAccountsToTaskChatsQueue
    ));

    services.AddHostedService<RabbitMqService>(sp => new RabbitMqService
    (
        sp.GetRequiredService<IServiceScopeFactory>(),
        hostname,
        username,
        password,
        createTaskChatResponseQueue
    ));
}

void ConfigureMiddleware(WebApplication app)
{
    app.UseCors();
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
}

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ContentDbContext>();
        context.Database.Migrate();
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