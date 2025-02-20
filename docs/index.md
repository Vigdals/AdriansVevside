---
layout: default
title: "Home"
---

#### Welcome to AdriansVevside

[![Board Status](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/_apis/work/boardbadge/2ef32e82-faee-4544-8345-31d6f01e5fa2)](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/_boards/board/t/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/Microsoft.RequirementCategory)

# GitHub Pages (What you are reading now)

The **docs/** folder serves as the core for the documentation and blog management:

- **_posts/**: A collection of Markdown files for blog posts, following the naming convention `YYYY-MM-DD-title.md`. Each post is written in Markdown and processed by Jekyll to render properly within the site.
- **index.md**: The main homepage of the site, which provides an overview of the project, its features, and links to blog posts.
- **default.html**: The primary layout file that structures the site's UI. We recently modified this to include:
  - A **sidebar navigation** similar to Docsify, making it easy to browse and access blog posts.
  - **GitHub-style code snippets** using `highlight.js`, ensuring that C# and other code blocks look exactly like they do on GitHub.
  - A **dark mode-friendly UI** with improved readability and contrast, enhancing the overall user experience.
  - Automatic **blog post detection**, ensuring that any new post added to `_posts/` appears in the sidebar navigation dynamically.

The overall workflow is straightforward: whenever a new post is added to `_posts/`, it is automatically indexed, listed in the sidebar, and accessible in the blog section without manual updates.

# AdriansVevside

**AdriansVevside** is a personal project aimed at delving into the capabilities of the .NET ecosystem, particularly focusing on web development using ASP.NET Core MVC. This website serves as a sandbox for experimenting with various technologies, implementing new features, and sharing insights through blog posts.

### Repository Structure

The project is organized as follows:

- **Adrians/**: Contains the main application code, including:
  - **Controllers/**: Manages the flow of the application by handling user input and interactions, processing them, and returning appropriate responses.
  - **Models/**: Defines the data structures and business logic.
  - **Views/**: Handles the presentation layer, rendering the user interface.
  - **wwwroot/**: Hosts static files such as CSS, JavaScript, and images.

## Key Features

- **ASP.NET Core MVC Framework**: Utilized for building a dynamic, testable, and maintainable web application.
- **FPL Integration**: Implements a controller that fetches and displays Fantasy Premier League deadlines by consuming the official FPL API.

## Live Site

Explore the live version of the site here: [https://adriansvevside.azurewebsites.net/](https://adriansvevside.azurewebsites.net/)

## Blog Posts on GitHub Pages

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