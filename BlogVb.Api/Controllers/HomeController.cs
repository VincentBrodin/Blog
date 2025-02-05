using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Services;
using BlogVb.Api.Tools;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using IO = System.IO;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("/")]
public class HomeController : ControllerBase {
	[HttpGet]
	[Route("")]
	public async Task<IActionResult> GetAsync(ILayoutRenderer layoutRenderer, IBlogCache blogCache, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/");
		List<BlogForRendering> blogs = await blogCache.GetAllBlogsForRenderingAsync();
		return Content(await layoutRenderer.RenderAsync("pages/home", bodyData: new { blogs }), Accepts.Html);
	}

	[HttpGet]
	[Route("{blogUrl}")]
	public async Task<IActionResult> GetBlogAsync(string blogUrl, ILayoutRenderer layoutRenderer, IBlogCache blogCache, ICookieVault cookieVault) {
		Blog? blog = await blogCache.GetBlogAsync(blogUrl, true);
		if(blog == null) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.NotFound), Accepts.Html);
		}
		else {
			cookieVault.Set(HttpContext, "came-from", $"/{blogUrl}");
			MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
				.UseAutoIdentifiers()
				.Build();
			return Content(await layoutRenderer.RenderAsync("pages/blog", bodyData: new { content = Markdown.ToHtml(blog.Content, pipeline) }), Accepts.Html);
		}
	}


	[HttpGet]
	[Route("about")]
	public async Task<IActionResult> GetAboutAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/about");
		return Content(await layoutRenderer.RenderAsync("pages/about"), Accepts.Html);
	}

	[HttpGet]
	[Route("images/{fileName}")]
	public IActionResult GetImage(string fileName) {
		var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "images", fileName);
		if(!IO::File.Exists(filePath))
			return NotFound();

		byte[] fileBytes = IO::File.ReadAllBytes(filePath);
		return File(fileBytes, Helper.GetFileContentType(filePath));
	}
}
