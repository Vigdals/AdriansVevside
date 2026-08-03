global using Adrians.Models;

using System.Net.Http.Headers;
using Adrians.Data;
using Adrians.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Database connection string
// =======================
var connectionString =
    builder.Configuration.GetConnectionString("DefaultConnection")
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"]
    ?? builder.Configuration["db-connection-adriansvevside"]
    ?? throw new InvalidOperationException(
        "Database connection string not found.");

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
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "AdriansVevside/1.0 (vigdalpi.duckdns.org)");

    client.DefaultRequestHeaders.Accept.ParseAdd(
        "application/json");
});

builder.Services.AddHttpClient("met.no", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "AdriansVevside/1.0 (contact: adrvig92@gmail.com)");

    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json"));
});

builder.Services.AddHttpClient("hackernews", client =>
{
    client.BaseAddress = new Uri(
        "https://hacker-news.firebaseio.com/");

    client.DefaultRequestHeaders.Accept.ParseAdd(
        "application/json");
});

builder.Services.AddHttpClient<NifsKampService>(client =>
{
    client.BaseAddress = new Uri("https://api.nifs.no/");

    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "AdriansVevside/1.0 (vigdalpi.duckdns.org)");

    client.DefaultRequestHeaders.Accept.ParseAdd(
        "application/json");
});

builder.Services.AddHttpClient<SimasTommekalenderService>(client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "AdriansVevside/1.0 (vigdalpi.duckdns.org)");

    client.DefaultRequestHeaders.Accept.ParseAdd(
        "application/json");
});

// =======================
// App-services
// =======================
builder.Services.AddScoped<FrostService>();
builder.Services.AddScoped<MeteorologiskInstituttKorttidsvarselService>();
builder.Services.AddScoped<PublicPiStatusService>();
builder.Services.AddScoped<RssFeedService>();

// NifsKampService og SimasTommekalenderService er registrerte
// som typed HttpClient-services via AddHttpClient<TService>().

// =======================
// Database
// =======================
// Raspberry Pi-oppsettet brukar MariaDB 11.4.
//
// Ikkje bruk ServerVersion.AutoDetect(connectionString) her.
// AutoDetect opnar ei databasetilkopling under oppstart.
// Dersom MariaDB ikkje er klar etter reboot, kan webappen stoppe
// før Kestrel byrjar å lytte.
var serverVersion =
    new MariaDbServerVersion(new Version(11, 4, 0));

builder.Services.AddDbContext<GameContext>(options =>
    options.UseMySql(connectionString, serverVersion));

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

var app = builder.Build();

// =======================
// Feilhandtering
// =======================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

// =======================
// Health check
// =======================
// Enkel liveness-sjekk for Docker/nginx.
// Denne sjekkar ikkje databasen.
app.MapGet("/healthz", () => Results.Ok("ok"));

// =======================
// Middleware
// =======================
// Nginx handterer HTTPS eksternt.
// Ved redirect-loop bør ForwardedHeaders konfigurerast.
app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseRouting();

// =======================
// Routing
// =======================
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();