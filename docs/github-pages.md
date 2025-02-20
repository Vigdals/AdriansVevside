---
layout: default
title: "GitHub Pages & Blog"
---

# GitHub Pages & Blog

The **docs/** folder serves as the core for the documentation and blog management:

- **_posts/**: A collection of Markdown files for blog posts, following the naming convention `YYYY-MM-DD-title.md`. Each post is written in Markdown and processed by Jekyll to render properly within the site.
- **index.md**: The main homepage of the site, which provides an overview of the project, its features, and links to blog posts.
- **default.html**: The primary layout file that structures the site's UI. 

## Features
- **Sidebar Navigation**
- **GitHub-style Code Snippets**
- **Dark Mode UI**
- **Auto-detected Blog Posts**

### Blog Posts

<ul>
  {% for post in site.posts %}
    <li>
      <a href="{{ post.url }}">{{ post.title }}</a>
      <small>{{ post.date | date: "%B %d, %Y" }}</small>
    </li>
  {% endfor %}
</ul>
