using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Newtonsoft.Json;

public class FotballDataApi
{
    private const string ApiUrl = "https://api.football-data.org/v4/teams/81/matches";
    private readonly string _apiKey;

    // Retrieve the API key from Azure Key Vault
    public FotballDataApi()
    {
        var keyVaultUrl = "https://VigdalsKeyVault.vault.azure.net/";
        var secretClient = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential());

        KeyVaultSecret apiKeySecret = secretClient.GetSecret("FootballDataApiKey");

        _apiKey = apiKeySecret.Value;
    }

    public async Task<List<Match>> GetUpcomingMatchesAsync()
    {
        using var client = new HttpClient();
        client.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);

        // Fetch matches for FC Barcelona with the correct API call
        var response = await client.GetStringAsync($"{ApiUrl}?status=SCHEDULED&limit=10");

        // Deserialize the response into the BarcelonaModel object
        var matchesResponse = JsonConvert.DeserializeObject<BarcelonaModel>(response);

        var upcomingMatches = new List<Match>();

        if (matchesResponse?.Matches != null)
            foreach (var match in matchesResponse.Matches)
                upcomingMatches.Add(new Match
                {
                    HomeTeam = match.HomeTeam.Name,
                    AwayTeam = match.AwayTeam.Name,
                    Date = match.UtcDate,
                    Status = match.Status,
                    HomeTeamLogo = match.HomeTeam.CrestUrl,
                    AwayTeamLogo = match.AwayTeam.CrestUrl,
                    HomeTeamShortName = match.HomeTeam.TeamShortName,
                    AwayTeamShortName = match.AwayTeam.TeamShortName
                });

        return upcomingMatches;
    }
}