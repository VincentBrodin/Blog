using Blog.Api.Models.Blogs;
using Blog.Api.Services;
using Blog.Api.Tools;
using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;
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

		return Content(await layoutRenderer.RenderAsync("pages/home", new { title = "[vinbro]" }, new { blogs, page, next, prev }), Accepts.Html);
	}

	[HttpPost]
	[Route("")]
	public async Task<IActionResult> PostAsync(ILayoutRenderer layoutRenderer, IViewCache viewCache, IBlogCache blogCache, ICookieVault cookieVault, [FromQuery] int page = 1) {
		int next = page + 1;
		int prev = page == 1 ? 1 : page - 1;

		int start = (page - 1) * MAX_BLOGS;
		int end = start + MAX_BLOGS;

		List<BlogForRendering> blogs = await blogCache.RangeBlogsForRenderingAsync(start, end);

		/*string html = await viewCache.GetViewAsync("pages/home");*/
		/*HandlebarsTemplate<object, object> layout = Handlebars.Compile(html);*/
		/*AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");*/
		/*return Content(layout(new { blogs, page, next, prev, account }), Accepts.Html);*/
		return Content(await layoutRenderer.RenderCleanAsync("pages/home", new { blogs, page, next, prev }), Accepts.Html);
	}


	[HttpGet]
	[Route("blog/{blogUrl}")]
	public async Task<IActionResult> GetBlogAsync(string blogUrl, ILayoutRenderer layoutRenderer, IBlogCache blogCache, ICookieVault cookieVault) {
		Models.Blogs.BlogModel? blog = await blogCache.GetBlogAsync(blogUrl, true);
		if(blog == null) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.NotFound), Accepts.Html);
		}
		else {
			cookieVault.Set(HttpContext, "came-from", $"/blog/{blogUrl}");
			MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
				.UseAutoIdentifiers()
				.Build();
			return Content(await layoutRenderer.RenderAsync("pages/blog", new { title = $"{blog.Name} - [vinbro]" }, new { blog = new BlogForRendering(blog), content = Markdown.ToHtml(blog.Content, pipeline) }), Accepts.Html);
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
	public async Task<IActionResult> GetImage(IImageCache imageCache, string fileName) {
		var filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "wwwroot", "images", fileName);
		byte[]? image = await imageCache.GetImageAsync(filePath);
		if(image == null) {
			return NotFound();
		}
		return File(image, Helper.GetFileContentType(filePath));

	}

	[HttpGet]
	[Route("blog/image/{fileName}")]
	public async Task<IActionResult> GetBlogImage(IImageCache imageCache, string fileName) {
		string filePath = Path.Combine(Program.BlogDirectory, fileName);
		byte[]? image = await imageCache.GetImageAsync(filePath);
		if(image == null) {
			return NotFound();
		}
		return File(image, Helper.GetFileContentType(filePath));
	}

	[HttpGet]
	[Route("content/{fileName}")]
	public async Task<IActionResult> GetContentImage(IImageCache imageCache, string fileName) {
		string filePath = Path.Combine(Program.ContentImageDirectory, fileName);
		byte[]? image = await imageCache.GetImageAsync(filePath);
		if(image == null) {
			return NotFound();
		}
		return File(image, Helper.GetFileContentType(filePath));
	}
}
