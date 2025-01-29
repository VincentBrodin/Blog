using BlogVb.Api.Models;
using BlogVb.Api.Services;
using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("/")]
public class HomeController : ControllerBase {
	[HttpGet]
	[Route("")]
	public IActionResult Get(ILayoutRenderer layoutRenderer, IBlogCache blogCache) {
		Blog[] blogs = blogCache.GetAllBlogs();
		return Content(layoutRenderer.Render("pages/home", new { loggedIn = true }, new { blogs }), Accepts.Html);
	}

	[HttpGet]
	[Route("{blogUrl}")]
	public IActionResult GetBlog(string blogUrl, ILayoutRenderer layoutRenderer, IBlogCache blogCache) {
		Blog? blog = blogCache.GetBlog(blogUrl);
		if(blog == null) {
			return Content("ERROR", Accepts.Html);
		}
		else {
			string html = Markdown.ToHtml(blog.Content);
			return Content(layoutRenderer.Render("pages/blog", bodyData: new { content = html }), Accepts.Html);
		}
	}


	[HttpGet]
	[Route("about")]
	public IActionResult GetAbout(ILayoutRenderer layoutRenderer) {
		return Content(layoutRenderer.Render("pages/about", new { name = "Vincent" }, new { name = "Brodin" }), Accepts.Html);
	}
}
