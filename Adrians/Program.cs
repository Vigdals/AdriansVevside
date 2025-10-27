global using Adrians.Models;
using Adrians.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

// NEW
using Microsoft.Extensions.Options;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Konfigurasjon (les frå appsettings, appsettings.{Environment}.json, user-secrets og miljøvariablar)
// =======================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // NEW: mappar FootballData__ApiKey -> FootballData:ApiKey

// NEW: bind FootballData-seksjonen til ein options-klasse du brukar i FotballDataApi
builder.Services.Configure<FootballDataOptions>(
    builder.Configuration.GetSection("FootballData"));

// Registrer HttpClient + din klient
builder.Services.AddHttpClient<FotballDataApi>(); // NEW

// =======================
// Connection String Setup (LocalDB)
// =======================
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// =======================
// Service Registrations
// =======================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDbContext<GameContext>(options =>
    options.UseSqlServer(connectionString));

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options =>
        options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

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
