---
layout: default
title: "How I made FPL deadliner"
date: 2025-02-20
---
You can find the FPL deadline at [https://vigdal.dev/Fpl](https://vigdal.dev/Fpl). The FPLController in AdriansVevside is responsible for fetching and displaying upcoming Fantasy Premier League (FPL) deadlines. It integrates with the official FPL API and processes the event data before rendering it in the ASP.NET MVC view.

## How It Works

The `FPLController` follows a structured approach to handle data retrieval and presentation:

1. `IndexAsync` Action
   - Acts as the entry point for displaying FPL deadlines.
   - Calls `GetDeadlines()` to fetch all FPL events.
   - Filters out past events and keeps only upcoming ones.
   - Passes the final list of future deadlines to the View.

2. `GetDeadlines` Method
   - Makes an HTTP request to the official Fantasy Premier League API.
   - Parses the JSON response to extract event details.
   - Returns a list of structured event objects.

It uses `ApiCall.DoApiCallAsync` (a helper method) to fetch the raw JSON. The returned data is then parsed with `System.Text.Json`, creating a list of strongly typed `FplMatchesModel.Event` objects. 

### Code Example

```csharp
using Adrians.Resources;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Text.Json;

namespace Adrians.Controllers
{
  public class FplController : Controller
  {
      public async Task<IActionResult> IndexAsync()
      {
          var allDeadlines = await GetDeadlines();

          // Filter to only future deadlines
          DateTime now = DateTime.UtcNow;
          var futureDeadlines = allDeadlines
              .Where(e => e.DeadlineTime > now)
              .ToList();

          return View(futureDeadlines);
      }

      public async Task<List<FplMatchesModel.Event>> GetDeadlines()
      {
          // Call FPL API
          const string apiUrl = "https://fantasy.premierleague.com/api/bootstrap-static/";
          var jsonResult = await ApiCall.DoApiCallAsync(apiUrl);

          // Parse JSON with System.Text.Json
          JsonElement rootElement = JsonSerializer.Deserialize<JsonElement>(jsonResult);
          var eventsArray = rootElement.GetProperty("events");

          var fplEvents = new List<FplMatchesModel.Event>();

          foreach (JsonElement e in eventsArray.EnumerateArray())
          {
              var eventId = e.GetProperty("id").GetInt32();
              var eventName = e.GetProperty("name").GetString();
              var deadlineTime = DateTime.Parse(
                  e.GetProperty("deadline_time").GetString()!,
                  null,
                  System.Globalization.DateTimeStyles.AdjustToUniversal
              );

              fplEvents.Add(new FplMatchesModel.Event
              {
                  Id = eventId,
                  Name = eventName,
                  DeadlineTime = deadlineTime
              });
          }

          return fplEvents;
      }
  }
}
```