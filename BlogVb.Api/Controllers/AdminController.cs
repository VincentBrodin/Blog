using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Services;
using BlogVb.Api.Tools;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase {

	[HttpGet]
	[Route("edit/{blogUrl}")]
	public async Task<IActionResult> GetCreateAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault, IBlogCache blogCache, string blogUrl) {
		cookieVault.Set(HttpContext, "came-from", $"/admin/edit/{blogUrl}");
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}
		else if(account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.Unauthorized), Accepts.Html);
		}

		Blog? blog = await blogCache.GetBlogAsync(blogUrl, true);
		if(blog == null) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.NotFound), Accepts.Html);
		}
		EditBlog editBlog = new(blog);
		return Content(await layoutRenderer.RenderAsync("pages/edit", bodyData: new { blog = editBlog }), Accepts.Html);
	}

	[HttpPost]
	[Route("edit/{blogUrl}")]
	public async Task<IActionResult> PostEditAsync(IBlogCache blogCache, ICookieVault cookieVault, [FromForm] EditBlog editBlog, string blogUrl) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Unauthorized();
		}
		else if(account.Role == 0) {
			return Unauthorized();
		}

		Blog? blog = await blogCache.GetBlogAsync(blogUrl);
		if(blog == null) {
			return NotFound();
		}

		await blog.UpdateAsync(editBlog);

		Response.Headers.Append("HX-Redirect", "/");
		return Redirect("/");
	}




	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> GetCreateAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/admin/create");
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}
		else if(account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.Unauthorized), Accepts.Html);
		}

		return Content(await layoutRenderer.RenderAsync("pages/create", new { account }), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreateAsync(IBlogCache blogCache, ICookieVault cookieVault, [FromForm] CreateBlog createBlog) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Unauthorized();
		}
		else if(account.Role == 0) {
			return Unauthorized();
		}

		Blog blog = await Blog.GenerateNewBlogAsync(createBlog, account.Username);
		await blogCache.CacheBlogAsync(blog);
		Response.Headers.Append("HX-Redirect", "/");
		return Redirect("/");
	}
}
