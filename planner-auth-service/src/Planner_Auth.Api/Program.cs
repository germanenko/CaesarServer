using System.Text;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.OAuth;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Planner_Auth.App.Service;
using Planner_Auth.Core.IRepository;
using Planner_Auth.Core.IService;
using Planner_Auth.Infrastructure.Data;
using Planner_Auth.Infrastructure.Repository;
using Planner_Auth.Infrastructure.Service;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

var host = GetEnvVar("HOST");
builder.WebHost.UseUrls(host);
builder.WebHost.UseKestrel(options =>
{
    options.ListenAnyIP(8888, listenOptions =>
    {
        listenOptions.UseHttps("/https/local.pfx", "*Scores");
    });
});

ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
ApplyMigrations(app);

app.MapGet("/", () => $"Auth server work");

app.Run();



string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var authDbConnectionString = GetEnvVar("AUTH_DB_CONNECTION_STRING");
    var passwordHashKey = GetEnvVar("PASSWORD_HASH_KEY");
    var corsAllowedOrigins = GetEnvVar("CORS_ALLOWED_ORIGINS");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var googleClientId = GetEnvVar("GOOGLE_CLIENT_ID");
    var googleClientSecret = GetEnvVar("GOOGLE_CLIENT_SECRET");

    var mailruClientId = GetEnvVar("MAILRU_CLIENT_ID");
    var mailruClientSecret = GetEnvVar("MAILRU_CLIENT_SECRET");
    var mailruRedirectUri = GetEnvVar("MAILRU_REDIRECT_URI");

    var rabbitMqHostname = GetEnvVar("RABBITMQ_HOSTNAME");
    var rabbitMqUserName = GetEnvVar("RABBITMQ_USERNAME");
    var rabbitMqPassword = GetEnvVar("RABBITMQ_PASSWORD");

    var mailCredentialsQueueName = GetEnvVar("RABBITMQ_MAIL_CREDENTIALS_QUEUE_NAME");
    var profileImageQueueName = GetEnvVar("RABBITMQ_PROFILE_IMAGE_QUEUE_NAME");
    var createChatQueueName = GetEnvVar("RABBITMQ_CREATE_CHAT_QUEUE_NAME");
    var initChatQueue = GetEnvVar("RABBITMQ_INIT_CHAT_QUEUE_NAME");

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

    services.AddDataProtection()
                .PersistKeysToFileSystem(new DirectoryInfo("/keys"))
                .SetApplicationName("planer_auth");

    services
        .AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
        })
        .AddCookie(options =>
                {
                    options.Cookie.SameSite = SameSiteMode.None;
                    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                    options.LoginPath = "/google-login";
                })

                .AddGoogle(options =>
                {
                    options.ClientSecret = googleClientSecret;
                    options.ClientId = googleClientId;
                    options.Scope.Add("https://mail.google.com");
                    options.SaveTokens = true;
                    options.AccessType = "offline";

                    options.CallbackPath = new PathString("/signin-google");
                    options.Events = new OAuthEvents
                    {
                        OnRedirectToAuthorizationEndpoint = context =>
                        {
                            var redirectUri = "http://localhost:8888/signin-google"; // Обратите внимание на http
                            context.Response.Redirect(context.RedirectUri.Replace("https://localhost:8888/signin-google", redirectUri));
                            return Task.CompletedTask;
                        }
                    };
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

    services.AddDistributedMemoryCache();
    services.AddSession(options =>
    {
        options.IdleTimeout = TimeSpan.FromMinutes(15);
        options.Cookie.HttpOnly = true;
        options.Cookie.IsEssential = true;
    });

    services.AddAuthorization();

    ConfigureSwagger(services);

    services.AddDbContext<AuthDbContext>(options =>
    {
        options.UseNpgsql(authDbConnectionString, builder =>
        {
            builder.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null);
        });
    });

    services.AddSingleton<IHashPasswordService, HashPasswordService>(sp => new HashPasswordService(passwordHashKey));
    services.AddSingleton<RabbitMqService>(provider =>
        {
            var scopeFactory = provider.GetRequiredService<IServiceScopeFactory>();
            return new RabbitMqService(
                scopeFactory,
                rabbitMqHostname,
                rabbitMqUserName,
                rabbitMqPassword,
                profileImageQueueName,
                createChatQueueName,
                provider.GetRequiredService<INotifyService>()
            );
        });
    services.AddScoped<IUserService, UserService>();

    services.AddSingleton<INotifyService, RabbitMqNotifyService>(sp => new RabbitMqNotifyService(
        rabbitMqHostname,
        rabbitMqUserName,
        rabbitMqPassword,
        initChatQueue,
        mailCredentialsQueueName
    ));

    services.AddSingleton<IMailRuTokenService, MailRuTokenService>(sp => new MailRuTokenService(
        mailruClientId,
        mailruClientSecret,
        mailruRedirectUri));

    services.AddSingleton<IJwtService, JwtService>(sp => new JwtService(
        jwtSecret,
        jwtIssuer,
        jwtAudience));


    services.AddScoped<IAuthService, AuthService>();
    services.AddScoped<IAccountRepository, AccountRepository>();

    services.AddHostedService(provider => provider.GetRequiredService<RabbitMqService>());
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
    app.UseSession();
    app.UseAuthorization();

    app.MapControllers();
}

void ApplyMigrations(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<AuthDbContext>();
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
            Title = "Planner Auth Api",
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