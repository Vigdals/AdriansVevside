global using Adrians.Models;
using System.Net.Http.Headers;
using Adrians.Data;
using Adrians.Services;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// builder er inngangspunktet. Set opp alt før appen startar
var builder = WebApplication.CreateBuilder(args);

// =======================
// Konfigurasjon
// =======================
builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
    .AddEnvironmentVariables();

// =======================
// Key Vault
// =======================
var keyVaultUrlString = builder.Configuration["KeyVault:Url"]
    ?? throw new InvalidOperationException("KeyVault:Url is not configured.");
builder.Configuration.AddAzureKeyVault(new Uri(keyVaultUrlString), new DefaultAzureCredential());

// Vel connection string basert på miljø
// I dev: LocalDB frå appsettings
// I prod: hemmeleg streng frå Key Vault
string connectionString;
if (builder.Environment.IsDevelopment())
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection not found.");
else
    connectionString = builder.Configuration["db-connection-adriansvevside"]
        ?? throw new InvalidOperationException("Key Vault secret 'db-connection-adriansvevside' not found.");

// =======================
// HTTP-klientar
// =======================
// AddHttpClient<T> bind ein typesatt klient til FotballDataApi
// DI-containeren gir denne klienten til FotballDataApi automatisk
builder.Services.Configure<FootballDataOptions>(builder.Configuration.GetSection("FootballData"));
builder.Services.AddHttpClient<FotballDataApi>(client =>
{
    client.DefaultRequestHeaders.Add("X-Auth-Token", builder.Configuration["FootballData:ApiKey"]);
});

// Named clients – hentast ut med httpClientFactory.CreateClient("frost") osb.
// Nyttig når same service brukar fleire ulike API-ar
builder.Services.AddMemoryCache(); // Delt cache for alle services
builder.Services.AddHttpClient("frost", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (vigdal.dev)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});
builder.Services.AddScoped<FrostService>();

builder.Services.AddHttpClient("met.no", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (contact: adrvig92@gmail.com)");
    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
});
builder.Services.AddScoped<MeteorologiskInstituttKorttidsvarselService>();

builder.Services.AddHttpClient("hackernews", client =>
{
    client.BaseAddress = new Uri("https://hacker-news.firebaseio.com/");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

// =======================
// RSS
// =======================
builder.Services.AddScoped<RssFeedService>();

// =======================
// Database / Identity
// =======================
// EF Core – set opp databasekontekstane med SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddDbContext<GameContext>(o => o.UseSqlServer(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

// Identity – innebygd brukar- og rollesystem
builder.Services.AddDefaultIdentity<IdentityUser>(o => o.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient(); // Generisk fallback-klient (kan fjernast om du ikkje brukar den direkte)

// =======================
// Bygg appen
// =======================
var app = builder.Build();

// Middleware-pipeline
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint(); // Viser EF-migrasjonsfeil i dev
}
else
{
    app.UseExceptionHandler("/Home/Error"); // Feilside i prod
    app.UseHsts(); // Tving HTTPS i nettlesaren
}

app.UseHttpsRedirection(); // Redirect HTTP → HTTPS
app.UseStaticFiles();      // Tener filer frå wwwroot (CSS, JS, bilete)
app.UseRouting();          // Finn riktig controller/action for URL-en
app.UseAuthentication();   // Er du innlogga?
app.UseAuthorization();    // Har du tilgang? (må kome ETTER authentication)

app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // For Identity-sidene (login, register osb.)

app.Run();