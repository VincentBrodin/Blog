using BlogVb.Api.Models.Accounts;
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
	public async Task<IActionResult> GetCreateAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/admin/create");
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}

		return Content(await layoutRenderer.RenderAsync("pages/create", new {account}), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreateAsync(IBlogCache blogCache, ICookieVault cookieVault, [FromForm] BlogFromPost blogFromPost) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}

		Console.WriteLine(JsonSerializer.Serialize(blogFromPost));
		Blog blog = await Blog.GenerateNewBlogAsync(blogFromPost);
		await blogCache.CacheBlogAsync(blog);
		return Redirect("/");
	}
}
