using Blog.Api.Models.Accounts;
using Blog.Api.Models.Blogs;
using Blog.Api.Services;
using Blog.Api.Tools;
using Microsoft.AspNetCore.Mvc;

namespace Blog.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase {

	[HttpGet]
	[Route("edit/{blogUrl}")]
	public async Task<IActionResult> GetCreateAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault, IBlogCache blogCache, string blogUrl) {
		cookieVault.Set(HttpContext, "came-from", $"/admin/edit/{blogUrl}");
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}
		else if(account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.Unauthorized), Accepts.Html);
		}

		BlogModel? blog = await blogCache.GetBlogAsync(blogUrl, true);
		if(blog == null) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.NotFound), Accepts.Html);
		}
		EditBlog editBlog = new(blog);
		return Content(await layoutRenderer.RenderAsync("pages/edit", new { title = "Edit blog - [vinbro]" }, new { blog = editBlog }), Accepts.Html);
	}

	[HttpPost]
	[Route("edit/{blogUrl}")]
	public async Task<IActionResult> PostEditAsync(IBlogCache blogCache, ICookieVault cookieVault, [FromForm] EditBlog editBlog, string blogUrl) {
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Unauthorized();
		}
		else if(account.Role == 0) {
			return Unauthorized();
		}

		BlogModel? blog = await blogCache.GetBlogAsync(blogUrl);
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
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}
		else if(account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.Unauthorized), Accepts.Html);
		}

		return Content(await layoutRenderer.RenderAsync("pages/create", new { account, title = "New blog - [vinbro]" }), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreateAsync(IBlogCache blogCache, ICookieVault cookieVault, [FromForm] CreateBlog createBlog) {
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Unauthorized();
		}
		else if(account.Role == 0) {
			return Unauthorized();
		}

		BlogModel blog = await BlogModel.GenerateNewBlogAsync(createBlog, account.Username);
		await blogCache.CacheBlogAsync(blog);
		Response.Headers.Append("HX-Redirect", "/");
		return Redirect("/");
	}
}
