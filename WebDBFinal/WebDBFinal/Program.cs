using Microsoft.EntityFrameworkCore;
using WebDBFinal.Context;
using WebDBFinal.Services;
using DotNetEnv;



var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

Env.Load("ENV.env");

string? serverName = Environment.GetEnvironmentVariable("SERVER_NAME");
string? databaseName = Environment.GetEnvironmentVariable("DATABASE_NAME");
string? password = Environment.GetEnvironmentVariable("PASSWORD_DB");

// Construir la connection string a partir de variables de entorno si están presentes
string? envConnectionString = null;
if (!string.IsNullOrWhiteSpace(serverName) && !string.IsNullOrWhiteSpace(databaseName) && !string.IsNullOrWhiteSpace(password))
{
    // Agrego TrustServerCertificate para evitar problemas con certificados en entornos locales
    envConnectionString = $"Server={serverName};Database={databaseName};User Id=sa;Password={password};TrustServerCertificate=True;";
}

// Preferir la cadena desde ENV, si no existe usar DefaultConnection del appsettings
string? finalConnectionString = envConnectionString ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(finalConnectionString))
{
    throw new InvalidOperationException("No se encontró ninguna cadena de conexión. Configure ENV.env o DefaultConnection en appsettings.json.");
}

// Configurar DbContext
builder.Services.AddDbContext<ResidencialesDbContext>(options =>
{
    options.UseSqlServer(finalConnectionString!, sqlOptions =>
    {
        // Habilitar reintentos por si hay transitorios en la conexión
        sqlOptions.EnableRetryOnFailure();
    });
    options.EnableSensitiveDataLogging();
    options.EnableDetailedErrors();
});

// Registrar el servicio de procedimientos almacenados
builder.Services.AddScoped<StoredProcedureService>();

// Registrar el servicio de llaves foráneas
builder.Services.AddScoped<ForeignKeyService>();

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}


app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();


app.Run();