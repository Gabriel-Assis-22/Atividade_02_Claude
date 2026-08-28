using System.Text;
using Application.UseCases;
using Domain.Repositories;
using Domain.Services;
using FluentMigrator.Runner;
using Infrastructure.Email;
using Infrastructure.Migrations;
using Infrastructure.Persistence;
using Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Variáveis de ambiente ──────────────────────────────────────────────────────
var dbHost     = Environment.GetEnvironmentVariable("DB_HOST")     ?? "localhost";
var dbPort     = Environment.GetEnvironmentVariable("DB_PORT")     ?? "3306";
var dbUser     = Environment.GetEnvironmentVariable("DB_USER")     ?? throw new Exception("DB_USER não definida");
var dbPassword = Environment.GetEnvironmentVariable("DB_PASSWORD") ?? throw new Exception("DB_PASSWORD não definida");
var dbName     = Environment.GetEnvironmentVariable("DB_NAME")     ?? throw new Exception("DB_NAME não definida");
var jwtSecret  = Environment.GetEnvironmentVariable("JWT_SECRET")  ?? throw new Exception("JWT_SECRET não definida");

var connString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};";

// ── Infrastructure ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton(new DbConnectionFactory(connString));
builder.Services.AddScoped<IUsuarioRepository, UsuarioRepository>();
builder.Services.AddScoped<IResetTokenRepository, ResetTokenRepository>();
builder.Services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();
builder.Services.AddHttpClient<IEmailService, MailtrapEmailService>();

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<LoginUseCase>();
builder.Services.AddScoped<RegisterUseCase>();
builder.Services.AddScoped<ForgotPasswordUseCase>();
builder.Services.AddScoped<ResetPasswordUseCase>();

// ── JWT (mesma chave do catálogo para validação cruzada) ───────────────────────
var keyBytes = Encoding.UTF8.GetBytes(jwtSecret);
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts =>
    {
        opts.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(keyBytes),
            ValidateIssuer = false,
            ValidateAudience = false,
            ClockSkew = TimeSpan.Zero,
        };
    });
builder.Services.AddAuthorization();

// ── CORS (só o backend catálogo precisa chamar internamente) ───────────────────
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ── FluentMigrator ─────────────────────────────────────────────────────────────
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(r => r
        .AddMySql5()
        .WithGlobalConnectionString(connString)
        .ScanIn(typeof(_001_CreateUsuarios).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddConsole());

// ── Controllers ────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Executar Migrations ────────────────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp(); // Idempotente
}

// ── Pipeline ───────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
