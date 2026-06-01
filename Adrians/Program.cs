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
// HTTP-klientar
// =======================
builder.Services.Configure<FootballDataOptions>(builder.Configuration.GetSection("FootballData"));
builder.Services.AddHttpClient<FotballDataApi>(client =>
{
    var apiKey = builder.Configuration["FootballData:ApiKey"];

    if (!string.IsNullOrWhiteSpace(apiKey))
    {
        client.DefaultRequestHeaders.Add("X-Auth-Token", apiKey);
    }
});

builder.Services.AddMemoryCache();

builder.Services.AddHttpClient("frost", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (vigdalpi.duckdns.org)");
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

builder.Services.AddScoped<RssFeedService>();

// =======================
// Database / Identity
// =======================
// Raspberry Pi-oppsettet ditt brukar MariaDB.
// Dette krev NuGet-pakken Pomelo.EntityFrameworkCore.MySql.
var serverVersion = ServerVersion.AutoDetect(connectionString);

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
// MVC
// =======================
builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

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

// Nginx handterer HTTPS eksternt, men appen får X-Forwarded-Proto.
// Denne kan stå, men dersom du får redirect-loop seinare må vi setje ForwardedHeaders.
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