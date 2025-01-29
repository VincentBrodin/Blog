# The Best Tech Stack for Building a Web Apps (in my opinion)
If you're anything like me, you've probably spent hours (or days, or maybe even weeks) figuring out the "perfect" tech stack for your personal projects.
Well, after much trial and error, I’ve found what allows me to work blazingly fast.
The stack I’m using to power my blog isn’t just solid, it’s fast, efficient, and—dare I say—*beautiful*. 
In this post, I’m going to walk you through the tech stack that powers my blog,
and why I think it's the best choice for any developer who wants a seamless, performant, and elegant solution for their own project.
But before we dive in—remember, this is *just my opinion*.
The best tech stack for you is the one that you're most comfortable with,
because having the "perfect" tools on paper is useless if you don’t know how to use them.
So, if you're more into Node.js, Go, or something else, go with that—just make sure you *own* your stack!

## Why ASP.NET Core Web API? Because I’m Comfortable with It (And It’s Fast)

Let’s start with the backbone: **ASP.NET Core Web API**. Now, you might be wondering—“Why not Go, Node.js, or something else?”
The truth is, I chose **ASP.NET Core** because it's what I’m most comfortable with.
After all, when you’re building something as personal as a blog,
you want to use the tools that allow you to work most efficiently, right?

But comfort aside, let me tell you—ASP.NET Core is *faster* than ever,
especially with the improvements in .NET 9. With these performance upgrades,
ASP.NET Core is not only lightweight(ish) and cross-platform, but it also handles high-throughput scenarios like a pro.
It’s perfect for serving dynamic content (like my blog posts) at lightning speed.

So yeah, ASP.NET Core wasn’t chosen just because I’m familiar with it,
but also because it has *seriously* impressive performance.
Whether you’re serving HTML, APIs, or handling complex logic, ASP.NET Core makes it all easy and fast.
And when you're building a blog that needs to scale or handle spikes in traffic,
you can't go wrong with a framework this powerful.

## Handlebars.Net for SSR: Because Static Is Overrated

Speaking of dynamic content, let’s talk about **Handlebars.Net**.
You might be thinking, “Server-side rendering (SSR)? Is that still a thing?”
Trust me, SSR is the way to go when you want your content to load *fast*, be easily crawled by search engines,
and deliver a more stable user experience.

With **Handlebars.Net**, I can create simple, maintainable templates on the server,
populate them with data, and then send the HTML straight to the client.
There's no need for complex client-side frameworks or JavaScript-heavy rendering.
The browser gets the page pre-rendered and ready to go, ensuring that the user experience is top-notch from the get-go. 

This also helps with SEO. Unlike client-rendered pages, search engines can crawl the content right from the initial HTML response,
meaning they can index my blog without issues. 

## Tailwind CSS + Daisy UI: Style Without the Struggle

Now, let’s talk about the *look* of the blog.
If you’re tired of writing custom CSS or wrestling with design frameworks that don't quite fit your needs,
let me introduce you to **Tailwind CSS**. 

Tailwind is a utility-first CSS framework, which means I can apply pre-defined classes directly in my HTML to style elements.
No more digging through large CSS files to figure out where a class is defined. It’s incredibly *efficient* and gives me total control over how things look without the typical mess of custom styles.

But Tailwind by itself isn’t the whole story. That’s where **Daisy UI** comes in. Daisy UI is a set of customizable UI components that are built with Tailwind CSS in mind. Think pre-designed buttons, forms, modals, and navigation elements that you can customize with just a few class changes. It’s the perfect combination of flexibility and speed.

With **Tailwind CSS** and **Daisy UI**, I get a modern, responsive design without reinventing the wheel. I’m able to create a clean, professional look for my blog in a fraction of the time.

## HTMX: No More Full Page Reloads

Let’s take this a step further. You know how it is—you click a link, and the entire page reloads. That’s fine for some websites, but not for mine. **HTMX** lets me *ditch* full page reloads and create dynamic, interactive pages without bloating my app with JavaScript.

HTMX allows me to update parts of my page asynchronously. So, if I’m adding a new post or submitting a comment, HTMX sends requests to the backend and dynamically updates the page with new content—without needing a page refresh. It's smooth, efficient, and helps keep the user experience fast.

For example, when a user scrolls through my blog, HTMX handles the loading of new posts automatically. If they interact with forms or buttons, it updates only the relevant sections of the page. No JavaScript frameworks, no complicated client-side logic—just HTML and a sprinkle of magic.

## Markdig: Markdown Meets the Web

Let’s talk content. I love Markdown—it’s simple, it’s fast, and it makes writing content feel like, well, *writing*. The only issue? You can’t serve Markdown directly to the browser. Enter **Markdig**, a powerful Markdown processor for .NET.

Markdig takes care of converting my Markdown files into HTML, allowing me to write in plain text and have it rendered perfectly on the web. This means I can maintain a smooth workflow where I focus on content creation, and Markdig handles the formatting. Plus, it supports all the fancy stuff like tables, code blocks, and images. I write once, and Markdig converts it into a polished web page.

## Why This Stack Works for Developers

By now, you might be wondering—what’s the catch? Is this stack actually practical, or is it just another *hipster* dev setup?

The truth is, this stack works for a lot of reasons:

- **ASP.NET Core** is fast and reliable, and with .NET 9, it’s even faster than before. It’s built for performance and can handle high traffic loads, making it perfect for a personal blog.
- **Handlebars.Net** offers easy-to-use server-side rendering that boosts both performance and SEO. It’s simple, yet powerful.
- **Tailwind CSS + Daisy UI** gives you total flexibility to design your blog without all the pain of custom styles.
- **HTMX** provides a lightweight, dynamic experience, keeping things fast without the need for heavy client-side frameworks.
- **Markdig** allows me to focus on writing without worrying about HTML or complex formatting.

This stack lets me focus on the things I care about—*writing content*—while ensuring the performance, style, and functionality of my site are top-notch. It’s efficient, fast, and gets the job done without unnecessary complexity.

## Conclusion: The Best Tech Stack for Your Blog?

So, what do you think? If you're looking for a modern, powerful, and straightforward tech stack for your own blog, I highly recommend giving this one a try. From the clean backend with ASP.NET Core to the fast frontend with Tailwind, Daisy UI, and HTMX, it’s a winning combination that scales well and looks great.

But remember, this is just my take on what works for me. The best tech stack for you is the one you’re most comfortable with and enjoy working with—because the perfect tools on paper are useless if you don’t know how to use them. Whether you're building your first blog or just looking to try something new, this tech stack is built for developers who want the best of both worlds: simplicity and power.

Give it a go, and let me know how it works for you!

Happy coding!
