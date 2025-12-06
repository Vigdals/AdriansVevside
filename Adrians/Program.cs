global using Adrians.Models;
using Adrians.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

// NYTT: Key Vault
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Konfigurasjon
// =======================
builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables(); // FootballData__ApiKey -> FootballData:ApiKey

// =======================
// Key Vault-integrasjon
// =======================

// Les Key Vault-URL frå config
var keyVaultUrlString = builder.Configuration["KeyVault:Url"]
    ?? throw new InvalidOperationException("KeyVault:Url is not configured.");

var keyVaultUrl = new Uri(keyVaultUrlString);

// Legg til Key Vault som config-provider
builder.Configuration.AddAzureKeyVault(keyVaultUrl, new DefaultAzureCredential());

// =======================
// Connection String (dev vs prod)
// =======================
string connectionString;

if (builder.Environment.IsDevelopment())
{
    // Lokal DB i utvikling
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection not found in configuration.");
}
else
{
    // Prod – connectionstring frå Key Vault
    connectionString = builder.Configuration["db-connection-adriansvevside"]
        ?? throw new InvalidOperationException("Key Vault secret 'db-connection-adriansvevside' not found.");
}

// =======================
// FootballData-options / HttpClient
// =======================

// Bind FootballData-seksjonen til options
builder.Services.Configure<FootballDataOptions>(
    builder.Configuration.GetSection("FootballData"));

// Registrer HttpClient + din klient
builder.Services.AddHttpClient<FotballDataApi>();

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
