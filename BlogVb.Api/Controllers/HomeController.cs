using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Services;
using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("/")]
public class HomeController : ControllerBase {
	[HttpGet]
	[Route("")]
	public async Task<IActionResult> GetAsync(ILayoutRenderer layoutRenderer, IBlogCache blogCache) {
		BlogForRendering[] blogs = blogCache.GetAllBlogsForRendering();
		return Content(await layoutRenderer.RenderAsync("pages/home", new { loggedIn = false }, new { blogs }), Accepts.Html);
	}

	[HttpGet]
	[Route("{blogUrl}")]
	public async Task<IActionResult> GetBlogAsync(string blogUrl, ILayoutRenderer layoutRenderer, IBlogCache blogCache) {
		Blog? blog = await blogCache.GetBlogAsync(blogUrl);
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
	public async Task<IActionResult> GetAboutAsync(ILayoutRenderer layoutRenderer) {
		return Content(await layoutRenderer.RenderAsync("pages/about", new { name = "Vincent" }, new { name = "Brodin" }), Accepts.Html);
	}
}
