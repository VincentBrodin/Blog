using BlogVb.Api.Models.Accounts;
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
	public async Task<IActionResult> GetAsync(ILayoutRenderer layoutRenderer, IBlogCache blogCache, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/");
		BlogForRendering[] blogs = blogCache.GetAllBlogsForRendering();
		return Content(await layoutRenderer.RenderAsync("pages/home", bodyData: new { blogs }), Accepts.Html);
	}

	[HttpGet]
	[Route("{blogUrl}")]
	public async Task<IActionResult> GetBlogAsync(string blogUrl, ILayoutRenderer layoutRenderer, IBlogCache blogCache, ICookieVault cookieVault) {
		Blog? blog = await blogCache.GetBlogAsync(blogUrl);
		if(blog == null) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.NotFound), Accepts.Html);
		}
		else {
			cookieVault.Set(HttpContext, "came-from", $"/{blogUrl}");
			return Content(await layoutRenderer.RenderAsync("pages/blog", bodyData: new { content = Markdown.ToHtml(blog.Content) }), Accepts.Html);
		}
	}


	[HttpGet]
	[Route("about")]
	public async Task<IActionResult> GetAboutAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/about");
		return Content(await layoutRenderer.RenderAsync("pages/about"), Accepts.Html);
	}
}
