using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Planer_email_service.App.Service;
using Planer_email_service.Core.IService;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);
ConfigureServices(builder.Services);

var app = builder.Build();

ConfigureMiddleware(app);
app.MapGet("/", () => $"Email server work");

app.Run();

string GetEnvVar(string name) => Environment.GetEnvironmentVariable(name) ?? throw new Exception($"{name} is not set");

void ConfigureServices(IServiceCollection services)
{
    var emailSenderName = GetEnvVar("EMAIL_SENDER_NAME");
    var emailSenderEmail = GetEnvVar("EMAIL_SENDER_EMAIL");
    var emailSmtpServer = GetEnvVar("EMAIL_SMTP_SERVER");
    var emailSmtpPort = int.Parse(GetEnvVar("EMAIL_SMTP_PORT"));
    var emailSenderPassword = GetEnvVar("EMAIL_SENDER_PASSWORD");

    var jwtSecret = GetEnvVar("JWT_AUTH_SECRET");
    var jwtIssuer = GetEnvVar("JWT_AUTH_ISSUER");
    var jwtAudience = GetEnvVar("JWT_AUTH_AUDIENCE");

    var authServiceBaseUrl = GetEnvVar("AUTH_SERVICE_BASE_URL");
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

    services.AddHttpClient<AccountService>(client =>
    {
        client.BaseAddress = new Uri(authServiceBaseUrl);
    });

    services.AddAuthorization();

    ConfigureSwagger(services);


    services.AddSingleton<IEmailService, EmailService>(sp => new EmailService(
        emailSenderEmail,
        emailSenderPassword,
        emailSenderName,
        emailSmtpServer,
        emailSmtpPort,
        sp.GetRequiredService<ILogger<EmailService>>()));

    services.AddSingleton<IJwtService, JwtService>();
    services.AddScoped<IAccountService, AccountService>(sp => new AccountService(
        authServiceBaseUrl,
        sp.GetRequiredService<ILogger<AccountService>>()));
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

void ConfigureSwagger(IServiceCollection services)
{
    services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Planner email service Api",
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