---
layout: default
title: "Home"
---

#### Welcome to my GitHub Pages!

[![Board Status](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/_apis/work/boardbadge/2ef32e82-faee-4544-8345-31d6f01e5fa2)](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/_boards/board/t/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/Microsoft.RequirementCategory)

### Live Site

Explore the live version of the site here: [https://adriansvevside.azurewebsites.net/](https://adriansvevside.azurewebsites.net/) or here: [https://vigdal.dev/](https://vigdal.dev/).

### Blog Posts

Stay updated with the latest posts:

<ul>
  {% for post in site.posts %}
    <li>
      <a href="{{ post.url }}">{{ post.title }}</a>
      <small>{{ post.date | date: "%B %d, %Y" }}</small>
    </li>
  {% endfor %}
</ul>

### 🌦 Live Weather in Sogndal

<div id="weather">Loading weather...</div>

<!-- ## ⚽ Upcoming FC Barcelona Matches

<div id="barca-matches">Loading matches...</div> -->

<script>
  // 🌦 Fetch Weather from Yr.no for Sogndal
  fetch("https://api.met.no/weatherapi/locationforecast/2.0/compact?lat=61.2296&lon=7.1015", {
    headers: { "User-Agent": "AdriansVevside/1.0" }
  })
  .then(response => response.json())
  .then(data => {
    const weatherElement = document.getElementById("weather");
    if (data && data.properties && data.properties.timeseries && data.properties.timeseries.length > 0) {
      const details = data.properties.timeseries[0].data.instant.details;
      const temperature = details.air_temperature;
      const windSpeed = details.wind_speed;
      const precipitation = data.properties.timeseries[0].data.next_1_hours?.details?.precipitation_amount || 0;
      weatherElement.innerHTML = `🌡️ Temperature: ${temperature}°C<br>💨 Wind Speed: ${windSpeed} m/s<br>🌧️ Precipitation (next hour): ${precipitation} mm`;
    } else {
      weatherElement.innerHTML = "Weather data is currently unavailable.";
    }
  })
  .catch(error => {
    console.error("Error fetching weather data:", error);
    document.getElementById("weather").innerHTML = "Error loading weather.";
  });
</script>