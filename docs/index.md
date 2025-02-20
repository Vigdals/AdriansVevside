---
layout: default
title: "Home"
---

# Welcome to AdriansVevside

[![Board Status](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/_apis/work/boardbadge/2ef32e82-faee-4544-8345-31d6f01e5fa2)](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/_boards/board/t/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/Microsoft.RequirementCategory)

## Overview

**AdriansVevside** is a personal project aimed at delving into the capabilities of the .NET ecosystem, particularly focusing on web development using ASP.NET Core MVC. This website serves as a sandbox for experimenting with various technologies, implementing new features, and sharing insights through blog posts.

## Repository Structure

The project is organized as follows:

- **Adrians/**: Contains the main application code, including:
  - **Controllers/**: Manages the flow of the application by handling user input and interactions, processing them, and returning appropriate responses.
  - **Models/**: Defines the data structures and business logic.
  - **Views/**: Handles the presentation layer, rendering the user interface.
- **wwwroot/**: Hosts static files such as CSS, JavaScript, and images.
- **_posts/**: A collection of Markdown files for blog posts, following the naming convention `YYYY-MM-DD-title.md`.
- **Adrians.sln**: The solution file that ties all components of the project together.

## Key Features

- **ASP.NET Core MVC Framework**: Utilized for building a dynamic, testable, and maintainable web application.
- **FPL Integration**: Implements a controller that fetches and displays Fantasy Premier League deadlines by consuming the official FPL API.
- **Blog Engine**: A simple blogging platform powered by Markdown files, allowing for easy content creation and management.

## Live Site

Explore the live version of the site here: [https://adriansvevside.azurewebsites.net/](https://adriansvevside.azurewebsites.net/)

## Blog Posts

Stay updated with the latest posts:

<ul>
  {% for post in site.posts %}
    <li>
      <a href="{{ post.url }}">{{ post.title }}</a>
      <small>{{ post.date | date: "%B %d, %Y" }}</small>
    </li>
  {% endfor %}
</ul>

## Maintainer

This project is maintained by [Adrian](https://github.com/vigdals).

## License

This project is licensed under the MIT License. For more details, refer to the [LICENSE.md](LICENSE.md) file.

## Usage

Feel free to explore the code, suggest improvements, or fork the repository for your own experiments. Contributions are welcome!

```javascript
// Sample JavaScript code snippet
const message = 'Hello, AdriansVevside!';
console.log(message);
