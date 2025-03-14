using System.Text.Json;
using Adrians.Resources;
using Microsoft.AspNetCore.Mvc;

namespace Adrians.Controllers;

public class HackerNews : Controller
{
    public async Task<IActionResult> IndexAsync()
    {
        //Just an init of the other code
        var hackerNewsModelList = await GetApiInfo();

        return View(hackerNewsModelList);
    }

    public async Task<List<HackerNewsModel>> GetApiInfo()
    {
        var hackerNewsModelList = new List<HackerNewsModel?>();

        var apiUrl = "https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty";
        var jsonResult = await ApiCall.DoApiCallAsync(apiUrl);
        var json = JsonSerializer.Deserialize<JsonElement>(jsonResult);
        var antall = Math.Min(json.GetArrayLength(), 10);


        //Getting 10 stories
        for (var i = 0; i < antall; i++)
        {
            var storyUrl = "https://hacker-news.firebaseio.com/v0/item/" + json[i].GetInt32() +
                           ".json?print=pretty";
            var jsonStoryResult = await ApiCall.DoApiCallAsync(storyUrl);

            var jsonHackerNewsStory = JsonSerializer.Deserialize<JsonElement>(jsonStoryResult);
            //Debug.WriteLine("The json story we are getting: " + jsonHackerNewsStory);

            //Wonky that i have to set variables, then trycatch them THEN put them into the model. But it works
            string url, by, title;
            int descendants, score, id;
            try
            {
                url = jsonHackerNewsStory.GetProperty("url").GetString();
                descendants = jsonHackerNewsStory.GetProperty("descendants").GetInt32();
                by = jsonHackerNewsStory.GetProperty("by").GetString();
                score = jsonHackerNewsStory.GetProperty("score").GetInt32();
                title = jsonHackerNewsStory.GetProperty("title").GetString();
                id = jsonHackerNewsStory.GetProperty("id").GetInt32();
            }
            catch (KeyNotFoundException)
            {
                url = "N/A";
                descendants = 0;
                by = "N/A";
                score = 0;
                title = "N/A";
                id = 0;
            }

            var model = new HackerNewsModel
            {
                by = by,
                descendants = descendants,
                score = score,
                title = title,
                url = url,
                id = id
            };
            hackerNewsModelList.Add(model);
        }

        //return hackerNewsModelList;
        return hackerNewsModelList.OrderByDescending(i => i.score).ToList();
    }
}