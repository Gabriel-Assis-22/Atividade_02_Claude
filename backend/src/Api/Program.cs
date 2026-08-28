using System.Text;
using Application.UseCases.Comments;
using Application.UseCases.Favorites;
using Domain.Repositories;
using FluentMigrator.Runner;
using Infrastructure.ExternalServices;
using Infrastructure.Migrations;
using Infrastructure.Persistence;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// ── Variáveis de ambiente ──────────────────────────────────────────────────────
var tmdbApiKey  = Environment.GetEnvironmentVariable("TMDB_API_KEY")  ?? throw new Exception("TMDB_API_KEY não configurada");
var dbHost      = Environment.GetEnvironmentVariable("DB_HOST")       ?? "localhost";
var dbPort      = Environment.GetEnvironmentVariable("DB_PORT")       ?? "3306";
var dbUser      = Environment.GetEnvironmentVariable("DB_USER")       ?? throw new Exception("DB_USER não configurada");
var dbPassword  = Environment.GetEnvironmentVariable("DB_PASSWORD")   ?? throw new Exception("DB_PASSWORD não configurada");
var dbName      = Environment.GetEnvironmentVariable("DB_NAME")       ?? throw new Exception("DB_NAME não configurada");
var jwtSecret   = Environment.GetEnvironmentVariable("JWT_SECRET")    ?? throw new Exception("JWT_SECRET não configurada");

var connString = $"Server={dbHost};Port={dbPort};Database={dbName};Uid={dbUser};Pwd={dbPassword};";

// ── Infrastructure ─────────────────────────────────────────────────────────────
builder.Services.AddSingleton(new DbConnectionFactory(connString));
builder.Services.AddScoped<IFavoritoRepository, FavoritoRepository>();
builder.Services.AddScoped<IComentarioRepository, ComentarioRepository>();

// ── TMDB HttpClient ────────────────────────────────────────────────────────────
builder.Services.AddHttpClient<TmdbService>(client =>
{
    client.BaseAddress = new Uri("https://api.themoviedb.org");
    client.DefaultRequestHeaders.Authorization =
        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", tmdbApiKey);
});

// ── Auth Service HttpClient (Comunicação Interna Docker) ──────────────────────
var authServiceUrl = Environment.GetEnvironmentVariable("AUTH_SERVICE_URL") ?? "http://auth-service:8081";
builder.Services.AddHttpClient("AuthService", client =>
{
    client.BaseAddress = new Uri(authServiceUrl);
});

// ── Use Cases ──────────────────────────────────────────────────────────────────
builder.Services.AddScoped<GetFavoritesUseCase>();
builder.Services.AddScoped<AddFavoriteUseCase>();
builder.Services.AddScoped<RemoveFavoriteUseCase>();
builder.Services.AddScoped<CheckFavoriteUseCase>();
builder.Services.AddScoped<GetCommentsUseCase>();
builder.Services.AddScoped<AddCommentUseCase>();

// ── JWT Auth ───────────────────────────────────────────────────────────────────
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

// ── CORS ───────────────────────────────────────────────────────────────────────
builder.Services.AddCors(opts =>
    opts.AddDefaultPolicy(p => p
        .WithOrigins("http://localhost:4200", "https://gabriel-assis-isw055.lapps.studio")
        .AllowAnyHeader()
        .AllowAnyMethod()));

// ── FluentMigrator ─────────────────────────────────────────────────────────────
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(r => r
        .AddMySql5()
        .WithGlobalConnectionString(connString)
        .ScanIn(typeof(_001_CreateUsuarios).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddConsole());

// ── Controllers + Swagger ─────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// ── Executar Migrations no Startup ─────────────────────────────────────────────
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    // MigrateUp() é idempotente — só aplica migrations ainda não registradas na tabela VersionInfo
    runner.MigrateUp();
}

// ── Pipeline ───────────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
