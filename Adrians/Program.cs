global using Adrians.Models;
using System.Net.Http.Headers;
using Adrians.Data;
using Adrians.Services;
using Azure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", true)
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
    connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("DefaultConnection not found in configuration.");
else
    connectionString = builder.Configuration["db-connection-adriansvevside"]
                       ?? throw new InvalidOperationException(
                           "Key Vault secret 'db-connection-adriansvevside' not found.");

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

// Named HttpClient for Frost
builder.Services.AddHttpClient("frost", client =>
{
    client.DefaultRequestHeaders.UserAgent.ParseAdd("AdriansVevside/1.0 (vigdal.dev)");
    client.DefaultRequestHeaders.Accept.ParseAdd("application/json");
});

builder.Services.AddScoped<FrostService>();

// Named HttpClient for met.no
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
    "default",
    "{controller=Home}/{action=Index}/{id?}");

app.MapRazorPages();

app.Run();