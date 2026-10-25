global using Adrians.Models;
using Adrians.Resources;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using Adrians.Services;

var builder = WebApplication.CreateBuilder(args);

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

builder.Services
    .AddOptions<UptimeKumaOptions>()
    .Bind(
        builder.Configuration.GetSection(
            UptimeKumaOptions.SectionName))
    .Validate(
        options =>
            !options.Enabled ||
            (
                Uri.TryCreate(
                    options.BaseUrl,
                    UriKind.Absolute,
                    out var uri) &&
                (
                    uri.Scheme == Uri.UriSchemeHttp ||
                    uri.Scheme == Uri.UriSchemeHttps
                )
            ),
        "UptimeKuma:BaseUrl må vere ei gyldig HTTP/HTTPS-adresse.")
    .Validate(
        options =>
            !options.Enabled ||
            !string.IsNullOrWhiteSpace(
                options.StatusPageSlug),
        "UptimeKuma:StatusPageSlug manglar.")
    .Validate(
        options =>
            options.CacheSeconds
            is >= 5 and <= 300,
        "UptimeKuma:CacheSeconds må vere mellom 5 og 300.")
    .ValidateOnStart();

builder.Services.AddHttpClient<UptimeKumaService>(
    (serviceProvider, client) =>
    {
        var options =
            serviceProvider
                .GetRequiredService<
                    IOptions<UptimeKumaOptions>>()
                .Value;

        if (options.Enabled)
        {
            var baseUrl =
                options.BaseUrl.TrimEnd('/') +
                "/";

            client.BaseAddress =
                new Uri(
                    baseUrl);
        }

        client.Timeout =
            TimeSpan.FromSeconds(
                5);

        client.DefaultRequestHeaders
            .UserAgent
            .ParseAdd(
                "AdriansVevside/1.0");

        client.DefaultRequestHeaders
            .Accept
            .ParseAdd(
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