---
layout: default
title: "Home"
---

#### Welcome to my GitHub Pages!

[![Board Status](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/_apis/work/boardbadge/2ef32e82-faee-4544-8345-31d6f01e5fa2)](https://dev.azure.com/adhvi/3093fd49-6d98-446c-b7ea-d0dcd7eda40d/_boards/board/t/114477e7-bf49-4fd4-bfdf-494c5ba0b07c/Microsoft.RequirementCategory)

## Live Site

Explore the live version of the site here: [https://adriansvevside.azurewebsites.net/](https://adriansvevside.azurewebsites.net/) or here: [https://vigdal.dev/](https://vigdal.dev/).

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