global using Adrians.Models;
using Adrians.Data;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// =======================
// Key Vault & Connection String Setup
// =======================

// Get the Key Vault URL from configuration
string keyVaultUrl = builder.Configuration["AzureKeyVault:VaultUri"];
// Get the base connection string (it should include a placeholder for the DB password)
string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;

if (!string.IsNullOrEmpty(keyVaultUrl))
{
    var kvUri = new Uri(keyVaultUrl);
    // Create a SecretClient to interact with Azure Key Vault
    var secretClient = new SecretClient(kvUri, new DefaultAzureCredential());

    // Add Azure Key Vault secrets to the configuration
    builder.Configuration.AddAzureKeyVault(secretClient, new AzureKeyVaultConfigurationOptions());

    // Retrieve the database password from Key Vault
    var secret = secretClient.GetSecret("sql-sa-adriansDbAdmin");
    var dbPassword = secret.Value.Value;

    // Replace the placeholder in the connection string with the actual password
    connectionString = connectionString.Replace("PLACEHOLDER", dbPassword);
}

// =======================
// Service Registrations
// =======================

// Register the ApplicationDbContext for Identity
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// Register the GameContext for your RPG game
builder.Services.AddDbContext<GameContext>(options =>
    options.UseSqlServer(connectionString));

// Additional services
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = false)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient();

// =======================
// Build and Configure the App
// =======================

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
