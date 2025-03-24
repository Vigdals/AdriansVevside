using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;

public class ChatGptService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public ChatGptService()
    {
        _httpClient = new HttpClient();

        // Hent API-nøkkel fra Key Vault
        var keyVaultUrl = "https://VigdalsKeyVault.vault.azure.net/";
        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());
        KeyVaultSecret apiKeySecret = secretClient.GetSecret("Api-ChatGPT-Key");
        _apiKey = apiKeySecret.Value;

        _httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {_apiKey}");
    }

    public async Task<string> GetBarcaSummaryAsync()
    {
        var requestBody = new
        {
            model = "gpt-4o",
            messages = new[]
            {
                new { role = "user", content = "Give me an in depth Daily Summary for FC Barcelona football team" }
            }
        };

        var content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json"
        );

        var response = await _httpClient.PostAsync("https://api.openai.com/v1/chat/completions", content);
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString();
    }
}