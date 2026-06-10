global using Adrians.Models;

using System.Net.Http.Headers;
using Adrians.Data;
using Adrians.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database connection string
// =======================
// Prioritet:
// 1. ConnectionStrings:DefaultConnection frå miljøvariabel / appsettings
// 2. db-connection-adriansvevside frå Key Vault, dersom Key Vault er aktivt
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? builder.Configuration["db-connection-adriansvevside"]
    ?? throw new InvalidOperationException("Database connection string not found.");

// =======================
// Cache
// =======================
builder.Services.AddMemoryCache();

// =======================
// HTTP-klientar / eksterne API
// =======================
builder.Services.Configure<FootballDataOptions>(
    builder.Configuration.GetSection("FootballData"));

builder.Services.AddHttpClient<FotballDataApi>(client =>
{
    var apiKey = builder.Configuration["FootballData:ApiKey"];

    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
    }
});

builder.Services.AddHttpClient("frost", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (vigdalpi.duckdns.org)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient("met.no", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (contact: adrvig92@gmail.com)");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("hackernews", client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<NifsKampService>(client =>
{
    client.BaseAddress = new Uri("https://api.nifs.no/");
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (vigdalpi.duckdns.org)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddHttpClient<SimasTommekalenderService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (vigdalpi.duckdns.org)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

// =======================
// App-services
// =======================
builder.Services.AddScoped<FrostService>();
builder.Services.AddScoped<MeteorologiskInstituttKorttidsvarselService>();
builder.Services.AddScoped<PublicPiStatusService>();
builder.Services.AddScoped<RssFeedService>();

// Merk:
// NifsKampService og SimasTommekalenderService er registrerte som typed HttpClient-services
// via AddHttpClient<TService>(). Dei treng normalt ikkje eigen AddScoped i tillegg.

// =======================
// Database / Identity
// =======================
// Raspberry Pi-oppsettet brukar MariaDB 11.4.
//
// Viktig:
// Ikkje bruk ServerVersion.AutoDetect(connectionString) her.
// AutoDetect opnar MySQL-tilkopling under app-startup. Dersom MariaDB ikkje er heilt klar
// akkurat ved reboot, døyr heile web-appen før Kestrel lyttar på 8080.
//
// Fast versjon gjer startup meir robust. Databasefeil kan framleis skje ved faktiske DB-kall,
// men appen kjem opp og nginx får ein levande upstream.
var serverVersion = new MariaDbServerVersion(new Version(11, 4, 0));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseMySql(connectionString, serverVersion));

builder.Services.AddDbContext<GameContext>(options =>
    options.UseMySql(connectionString, serverVersion));

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDatabaseDeveloperPageExceptionFilter();
}

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = false;
})
.AddRoles<IdentityRole>()
.AddEntityFrameworkStores<ApplicationDbContext>();

// =======================
// MVC / Razor
// =======================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =======================
// Middleware
// =======================
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// Enkel liveness-sjekk for Docker/nginx.
// Denne skal ikkje sjekke database. Poenget er å vite at Kestrel faktisk lyttar.
app.MapGet("/healthz", () => Results.Ok("ok"));

// Nginx handterer HTTPS eksternt.
// Dersom du får redirect-loop bak nginx, bør vi heller setje opp ForwardedHeaders eksplisitt.
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();