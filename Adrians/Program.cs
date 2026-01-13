global using Adrians.Models;
using Adrians.Data;
using Adrians.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

// NYTT: Key Vault
using Azure.Identity;
using Azure.Extensions.AspNetCore.Configuration.Secrets;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
    .AddEnvironmentVariables();

// =======================
// Key Vault
// =======================
var keyVaultUrlString = builder.Configuration["KeyVault:Url"]
    ?? throw new InvalidOperationException("KeyVault:Url is not configured.");

var keyVaultUrl = new Uri(keyVaultUrlString);

builder.Configuration.AddAzureKeyVault(
    keyVaultUrl,
    new DefaultAzureCredential()
);

string connectionString;

if (builder.Environment.IsDevelopment())
{
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
        ?? throw new InvalidOperationException("DefaultConnection not found in configuration.");
}
else
{
    connectionString = builder.Configuration["db-connection-adriansvevside"]
        ?? throw new InvalidOperationException(
            "Key Vault secret 'db-connection-adriansvevside' not found.");
}

// =======================
// FootballData-options / HttpClient
// =======================
builder.Services.Configure<FootballDataOptions>(
    builder.Configuration.GetSection("FootballData"));

builder.Services.AddHttpClient<FotballDataApi>();

// =======================
// MET.no Nowcast
// =======================

// Cache (krav for å ikkje spamme MET)
builder.Services.AddMemoryCache();

// Named HttpClient for met.no (VIKTIG)
builder.Services.AddHttpClient("met.no", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd(
        "AdriansVevside/1.0 (contact: adrvig92@gmail.com)"
    );
    client.DefaultRequestHeaders.Accept.Add(
        new MediaTypeWithQualityHeaderValue("application/json")
    );
});


builder.Services.AddScoped<MeteorologiskInstituttKorttidsvarselService>();

// =======================
// EF / Identity
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

// =======================
// MVC
// =======================
builder.Services.AddControllersWithViews();

// (global HttpClient er OK å ha i tillegg)
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
