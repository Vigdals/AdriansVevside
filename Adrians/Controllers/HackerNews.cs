using Microsoft.AspNetCore.Mvc;
using Adrians.Resources;
using System.Text.Json;
using System.Diagnostics;
using Adrians.Models;
using System;

namespace Adrians.Controllers
{
    public class HackerNews : Controller
    {
        public IActionResult Index()
        {
            //Just an init of the other code
            var listOfHackerNewsModels = GetApiInfo();

            return View(listOfHackerNewsModels);
        }
        public List<HackerNewsModel> GetApiInfo()
        {
            var ListOfHackerNewsModels = new List<HackerNewsModel>();

            string apiUrl = "https://hacker-news.firebaseio.com/v0/topstories.json?print=pretty";
            var jsonResult = ApiCall.DoApiCall(apiUrl);
            JsonElement json = JsonSerializer.Deserialize<JsonElement>(jsonResult);
            int antall = Math.Min(json.GetArrayLength(), 10);

            //Getting 10 stories
            for (int i = 0; i < antall; i++)
            {
                var storyUrl = "https://hacker-news.firebaseio.com/v0/item/" + json[i].GetInt32() +
                    ".json?print=pretty";
                var jsonStoryResult = ApiCall.DoApiCall(storyUrl);
                JsonElement jsonHackerNewsStory = JsonSerializer.Deserialize<JsonElement>(jsonStoryResult);
                Debug.WriteLine("The json story we are getting: " + jsonHackerNewsStory);

                //Wonky that i have to set variables, then trycatch them THEN put them into the model. But it works
                string url, by, title;
                int descendants, score;
                try
                {
                    url = jsonHackerNewsStory.GetProperty("url").GetString();
                    descendants = jsonHackerNewsStory.GetProperty("descendants").GetInt32();
                    by = jsonHackerNewsStory.GetProperty("by").GetString();
                    score = jsonHackerNewsStory.GetProperty("score").GetInt32();
                    title = jsonHackerNewsStory.GetProperty("title").GetString();
                }
                catch(KeyNotFoundException) { url = "N/A"; descendants = 0; by = "N/A"; score = 0; title = "N/A"; }

                var model = new HackerNewsModel()
                {
                    by = by,
                    descendants = descendants,
                    score = score,
                    title = title,
                    url = url
                };
                ListOfHackerNewsModels.Add(model);
            }

            return ListOfHackerNewsModels.OrderByDescending(i=>i.score).ToList();
        }
    }
}
