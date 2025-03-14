using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Adrians.Models;

public class FotballDataApi
{
    private const string ApiUrl = "https://api.football-data.org/v4/teams/81/matches"; // Correct URL for FC Barcelona
    private readonly string _apiKey = "4f5a263e32034f429ec2bf5b3cfce2b2"; // Your API key

    public async Task<List<Match>> GetUpcomingMatchesAsync()
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);

            // Fetch matches for FC Barcelona with the correct API call
            var response = await client.GetStringAsync($"{ApiUrl}?status=SCHEDULED&limit=5");

            // Deserialize the response into the BarcelonaModel object
            var matchesResponse = JsonConvert.DeserializeObject<BarcelonaModel>(response);

            var upcomingMatches = new List<Match>();

            if (matchesResponse?.Matches != null)
            {
                foreach (var match in matchesResponse.Matches)
                {
                    upcomingMatches.Add(new Match
                    {
                        HomeTeam = match.HomeTeam.Name,
                        AwayTeam = match.AwayTeam.Name,
                        Date = match.UtcDate,
                        Status = match.Status,
                        HomeTeamLogo = match.HomeTeam.CrestUrl, // Adding logo URL
                        AwayTeamLogo = match.AwayTeam.CrestUrl, // Adding logo URL
                        HomeTeamShortName = match.HomeTeam.TeamShortName, // Adding short name
                        AwayTeamShortName = match.AwayTeam.TeamShortName  // Adding short name
                    });
                }
            }

            return upcomingMatches;
        }
    }
}