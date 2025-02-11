using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Services;
using BlogVb.Api.Tools;
using HandlebarsDotNet;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using IO = System.IO;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("/")]
public class HomeController : ControllerBase {

	private const int MAX_BLOGS = 8;

	[HttpGet]
	[Route("")]
	public async Task<IActionResult> GetAsync(ILayoutRenderer layoutRenderer, IBlogCache blogCache, ICookieVault cookieVault, [FromQuery] int page = 1) {
		cookieVault.Set(HttpContext, "came-from", "/");
		int next = page + 1;
		int prev = page == 1 ? 1 : page - 1;

		int start = (page - 1) * MAX_BLOGS;
		int end = start + MAX_BLOGS;

		List<BlogForRendering> blogs = await blogCache.RangeBlogsForRenderingAsync(start, end);
		Console.WriteLine(blogs.Count);

		return Content(await layoutRenderer.RenderAsync("pages/home", new { title = "[vinbro]" }, new { blogs, page, next, prev }), Accepts.Html);
	}

	[HttpPost]
	[Route("")]
	public async Task<IActionResult> PostAsync(IViewCache viewCache, IBlogCache blogCache, ICookieVault cookieVault, [FromQuery] int page = 1) {
		int next = page + 1;
		int prev = page == 1 ? 1 : page - 1;

		int start = (page - 1) * MAX_BLOGS;
		int end = start + MAX_BLOGS;

		List<BlogForRendering> blogs = await blogCache.RangeBlogsForRenderingAsync(start, end);

		string html = await viewCache.GetViewAsync("pages/home");
		HandlebarsTemplate<object, object> layout = Handlebars.Compile(html);

		Account? account = cookieVault.Get<Account>(HttpContext, "user");

		return Content(layout(new { blogs, page, next, prev, account }), Accepts.Html);
	}


	[HttpGet]
	[Route("blog/{blogUrl}")]
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
			return Content(await layoutRenderer.RenderAsync("pages/blog", new { title = $"{blog.Name} - [vinbro]" }, new { content = Markdown.ToHtml(blog.Content, pipeline) }), Accepts.Html);
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
