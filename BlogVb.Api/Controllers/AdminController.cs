using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Services;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase {

	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> GetCreateAsync(ILayoutRenderer layoutRenderer) {
		return Content(await layoutRenderer.RenderAsync("pages/create"), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreateAsync(IBlogCache blogCache, [FromForm] BlogFromPost blogFromPost) {
		Console.WriteLine(JsonSerializer.Serialize(blogFromPost));
		Blog blog = await Blog.GenerateNewBlogAsync(blogFromPost);
		await blogCache.CacheBlogAsync(blog);
		return Redirect("/");
	}
}
