using Blog.Api.Models.Accounts;
using Blog.Api.Models.Blogs;
using Blog.Api.Models.Images;
using Blog.Api.Services;
using Blog.Api.Tools;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using IO = System.IO;

namespace Blog.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase {
	#region Edit
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
	#endregion
	#region Create
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
	#endregion
	#region Content
	[HttpGet]
	[Route("content")]
	public async Task<IActionResult> GetContentAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", "/admin/content");
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}
		else if(account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.Unauthorized), Accepts.Html);
		}

		if(!Directory.Exists(Program.ContentImageDirectory)) {
			Directory.CreateDirectory(Program.ContentImageDirectory);
		}
		string[] rawImagePaths = Directory.GetFiles(Program.ContentImageDirectory);
		List<string> images = [.. rawImagePaths.Select(p => Path.GetFileName(p))];
		return Content(await layoutRenderer.RenderAsync("pages/content", new { account, title = "Content - [vinbro]" }, new { images }), Accepts.Html);
	}

	[HttpPost]
	[Route("content")]
	public async Task<IActionResult> PostContentAsync(ILayoutRenderer layoutRenderer, ICookieVault cookieVault, [FromForm] AddImage addImage) {
		cookieVault.Set(HttpContext, "came-from", "/admin/content");
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null || account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorCleanAsync(WebError.Unauthorized), Accepts.Html);
		}

		if(addImage.Content != null) {
			string imageName = $"{addImage.Name}{Path.GetExtension(addImage.Content.FileName)}";
			string imagePath = Path.Combine(Program.ContentImageDirectory, imageName);

			await using Stream fileStream = new FileStream(imagePath, FileMode.Create);
			await addImage.Content.CopyToAsync(fileStream);

			if(!Directory.Exists(Program.ContentImageDirectory)) {
				Directory.CreateDirectory(Program.ContentImageDirectory);
			}
			string[] rawImagePaths = Directory.GetFiles(Program.ContentImageDirectory);
			List<string> images = [.. rawImagePaths.Select(p => Path.GetFileName(p))];
			return Content(await layoutRenderer.RenderCleanAsync("pages/content", new { images }), Accepts.Html);
		}
		return Content(await layoutRenderer.RenderErrorCleanAsync(WebError.BadRequest), Accepts.Html);
	}

	[HttpPost]
	[Route("content/delete/{imageName}")]
	public async Task<IActionResult> PostContentAsync(ILayoutRenderer layoutRenderer, IImageCache imageCache, ICookieVault cookieVault, [FromRoute] string imageName) {
		cookieVault.Set(HttpContext, "came-from", "/admin/content");
		AccountModel? account = cookieVault.Get<AccountModel>(HttpContext, "user");
		if(account == null || account.Role == 0) {
			return Content(await layoutRenderer.RenderErrorCleanAsync(WebError.Unauthorized), Accepts.Html);
		}

		string imagePath = Path.Combine(Program.ContentImageDirectory, imageName);
		imageCache.RemoveImage(imagePath);
		if(IO::File.Exists(imagePath)) {
			IO::File.Delete(imagePath);
			if(!Directory.Exists(Program.ContentImageDirectory)) {
				Directory.CreateDirectory(Program.ContentImageDirectory);
			}
			string[] rawImagePaths = Directory.GetFiles(Program.ContentImageDirectory);
			List<string> images = [.. rawImagePaths.Select(p => Path.GetFileName(p))];
			return Content(await layoutRenderer.RenderCleanAsync("pages/content", new { images }), Accepts.Html);
		}

		return Content(await layoutRenderer.RenderErrorCleanAsync(WebError.BadRequest), Accepts.Html);
	}
	#endregion
}
