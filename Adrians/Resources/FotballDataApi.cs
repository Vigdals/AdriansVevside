using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class FotballDataApi
{
    private const string ApiUrl = "https://api.football-data.org/v4/teams/81/matches";  // Correct URL for FC Barcelona
    private readonly string _apiKey = "4f5a263e32034f429ec2bf5b3cfce2b2";  // Your API key

    public async Task<List<Match>> GetUpcomingMatchesAsync()
    {
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("X-Auth-Token", _apiKey);

            // Fetch matches for FC Barcelona with the correct API call
            var response = await client.GetStringAsync($"{ApiUrl}?status=SCHEDULED&limit=5");

            // Deserialize the response into the list of Match objects
            var matchesResponse = JsonConvert.DeserializeObject<ApiMatchesResponse>(response);

            var upcomingMatches = new List<Match>();

            foreach (var match in matchesResponse.Matches)
            {
                upcomingMatches.Add(new Match
                {
                    HomeTeam = match.HomeTeam.Name,
                    AwayTeam = match.AwayTeam.Name,
                    Date = match.utcDate,
                    Status = match.Status,
                    HomeTeamLogo = match.HomeTeam.CrestUrl,  // Adding logo URL
                    AwayTeamLogo = match.AwayTeam.CrestUrl   // Adding logo URL
                });
            }

            return upcomingMatches;
        }
    }
}

public class ApiMatchesResponse
{
    public List<MatchDetail> Matches { get; set; }
}

public class MatchDetail
{
    public Team HomeTeam { get; set; }
    public Team AwayTeam { get; set; }
    public string utcDate { get; set; }
    public string Status { get; set; }
}

public class Team
{
    public string Name { get; set; }
    public string CrestUrl { get; set; }  // Property for the team logo URL
}
