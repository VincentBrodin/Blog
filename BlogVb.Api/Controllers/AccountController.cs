using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Services;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase {
	[HttpGet]
	[Route("login")]
	public async Task<IActionResult> GetLogin(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account != null) {
			Console.WriteLine("Logged in");
			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
				return Redirect("/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
				return Redirect(cameFrom);
			}
		}

		return Content(await layoutRenderer.RenderAsync("pages/login"), Accepts.Html);
	}

	[HttpPost]
	[Route("login")]
	public async Task<IActionResult> PostLogin(IAccountService accountService, IViewCache viewCache, ICookieVault cookieVault, [FromForm] LoginAccount loginAccount) {
		// First check if the user already is logged in?
		Account? cAccount = cookieVault.Get<Account>(HttpContext, "user");
		if(cAccount != null) {
			Console.WriteLine("Logged in");
			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
			}
		}

		Account? account = await accountService.GetByEmail(loginAccount.Email);
		if(account == null || account.Password != loginAccount.Password) {
			string renderedHtml = Handlebars.Compile(await viewCache.GetViewAsync("pages/login"))(new {
				email = loginAccount.Email,
				password = loginAccount.Password,
				invalid = true
			});
			return Content(renderedHtml, Accepts.Html);
		}
		else {
			cookieVault.Set(HttpContext, "user", account);

			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
			}
			return Ok();
		}
	}


	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> GetCreate(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account != null) {
			Console.WriteLine("Logged in");
			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
				return Redirect("/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
				return Redirect(cameFrom);
			}
		}
		return Content(await layoutRenderer.RenderAsync("pages/register"), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreate(IViewCache viewCache, IAccountService accountService, ICookieVault cookieVault, [FromForm] CreateAccount createAccount) {
		// Stop user from creating an account while signed in
		Account? cAccount = cookieVault.Get<Account>(HttpContext, "user");
		if(cAccount != null) {
			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
			}
		}

		bool accountExists = accountService.GetByEmail(createAccount.Email) != null;
		object? validate = createAccount.Validate(accountExists);
		// If no errors and no accounts with that email
		if(validate == null) {
			Account account = createAccount.GetAccount();
			await accountService.Add(account);
			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
			}
			return Ok();
		}
		else {
			string renderedHtml = Handlebars.Compile(await viewCache.GetViewAsync("pages/register"))(validate);
			return Content(renderedHtml, Accepts.Html);
		}
	}

	[HttpGet]
	[Route("logout")]
	public IActionResult GetLogout(ICookieVault cookieVault) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account != null) {
			cookieVault.Remove(HttpContext, "user");
		}

		string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
		if(cameFrom == null) {
			Response.Headers.Append("HX-Redirect", "/");
			return Redirect("/");
		}
		else {
			Response.Headers.Append("HX-Redirect", cameFrom);
			return Redirect(cameFrom);
		}
	}

}
