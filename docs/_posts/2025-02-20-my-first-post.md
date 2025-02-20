---
layout: post
title: "My First Tech Post"
date: 2025-02-20
---
Welcome to my new tech blog! Let’s talk about .NET, security, and everything in between.

# How I Used ASP.NET MVC in AdriansVevside

My _AdriansVevside_ website is built using **ASP.NET Core MVC**, which helps structure the application into three main components:

- **Models**: Define the data structures and business logic.  
- **Views**: Handle the presentation layer (HTML, CSS, etc.).  
- **Controllers**: Orchestrate data retrieval, apply logic, and direct results to the Views.

## FPLController Overview

One of the key parts of this site is the **FPLController**, which fetches upcoming Fantasy Premier League deadlines. Below is a quick look at how it works:

- **`IndexAsync` Action**  
  This action serves as the entry point for displaying future FPL deadlines. It calls `GetDeadlines()` to fetch all deadlines, then filters out events that have already passed, ensuring that only **future** deadlines appear to users.

- **`GetDeadlines` Method**  
  This method reaches out to the official Fantasy Premier League API:


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
