using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Planer_mailbox_service.App.Service;
using Planer_mailbox_service.Core.IService;
using Planer_mailbox_service.Infrastructure.Data;
using Planer_mailbox_service.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
ApplyMigrations(app);

app.MapGet("/", () => $"Mailbox server work");

app.Run();



string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var connectionString = GetEnvVar("MAIL_CREDENTIALS_DB_CONNECTION_STRING");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var googleClientId = GetEnvVar("GOOGLE_CLIENT_ID");
    var googleClientSecret = GetEnvVar("GOOGLE_CLIENT_SECRET");

    var mailRuClientId = GetEnvVar("MAILRU_CLIENT_ID");
    var mailRuClientSecret = GetEnvVar("MAILRU_CLIENT_SECRET");
    var mailRuRedirectUri = GetEnvVar("MAILRU_REDIRECT_URI");

    var hostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var queueName = GetEnvVar("RABBITMQ_QUEUE_NAME");
    var userName = GetEnvVar("RABBITMQ_USERNAME");
    var password = GetEnvVar("RABBITMQ_PASSWORD");

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
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
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

    services.AddDbContext<AccountCredentialsDbContext>(options => options.UseNpgsql(connectionString));

    services.AddAuthorization();

    ConfigureSwagger(services);

    services.AddScoped<IMailCredentialsService, MailCredentialsService>();

    services.AddSingleton<IMailboxService, MailboxService>();
    services.AddSingleton<IJwtService, JwtService>();
    services.AddSingleton<IGoogleTokenService, GoogleTokenService>(sp => new GoogleTokenService(
        googleClientId,
        googleClientSecret));
    services.AddSingleton<IMailRuTokenService, MailRuTokenService>(sp => new MailRuTokenService(
        mailRuClientId,
        mailRuClientSecret,
        mailRuRedirectUri));
    services.AddSingleton<ITokenService, TokenService>();
    services.AddHostedService<RabbitMqService>(sp => new RabbitMqService(
        sp.GetRequiredService<IServiceScopeFactory>(),
        hostname,
        queueName,
        userName,
        password
    ));
}

void ConfigureMiddleware(WebApplication app)
{
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseRouting();
    app.UseCors();

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
        var context = services.GetRequiredService<AccountCredentialsDbContext>();
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
            Title = "Planner Mailbox Service Api",
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