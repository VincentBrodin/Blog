using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Services;
using BlogVb.Api.Tools;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Http.Metadata;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase {

	private readonly ILogger<AccountController> logger;

	public AccountController(ILogger<AccountController> logger) {
		this.logger = logger;
	}

	[HttpGet]
	public async Task<IActionResult> Get(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		cookieVault.Set(HttpContext, "came-from", $"/account");
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}

		return Content(await layoutRenderer.RenderAsync("pages/account", new { title = $"{account.Username} - [vinbro]" }), Accepts.Html);
	}

	[HttpPost]
	public async Task<IActionResult> Post(ILayoutRenderer layoutRenderer, IAccountService accountService, ICookieVault cookieVault, [FromForm] string username) {
		cookieVault.Set(HttpContext, "came-from", $"/account");
		Account? cAccount = cookieVault.Get<Account>(HttpContext, "user");
		if(cAccount == null) {
			Response.Headers.Append("HX-Redirect", "/account/login");
			return Redirect("/account/login");
		}

		Account? account = await accountService.GetById(cAccount.Id);
		if(account == null) {
			return Content(await layoutRenderer.RenderErrorAsync(WebError.BadRequest), Accepts.Html);
		}

		account.Username = username;
		cAccount.Username = username;
		await accountService.Update(account);

		Response.Headers.Append("HX-Redirect", "/account");
		return Redirect("/account");

	}


	[HttpGet]
	[Route("login")]
	public async Task<IActionResult> GetLogin(ILayoutRenderer layoutRenderer, ICookieVault cookieVault) {
		Account? account = cookieVault.Get<Account>(HttpContext, "user");
		if(account != null) {
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

		return Content(await layoutRenderer.RenderAsync("pages/login", new { title = "Login - [vinbro]" }), Accepts.Html);
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
			logger.LogInformation("User failed to login");
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

			logger.LogInformation($"User {account.Id} logged in");
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
		return Content(await layoutRenderer.RenderAsync("pages/register", new { title = "Register - [vinbro]" }), Accepts.Html);
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
			// Auto sets me to owner
			if(account.Email == "vincent.brodin21@gmail.com") {
				account.Role = Account.Roles.Owner;
			}
			await accountService.Add(account);
			cookieVault.Set(HttpContext, "user", account);

			string? cameFrom = cookieVault.Get<string>(HttpContext, "came-from");
			if(cameFrom == null) {
				Response.Headers.Append("HX-Redirect", "/");
			}
			else {
				Response.Headers.Append("HX-Redirect", cameFrom);
			}

			logger.LogInformation($"User {account.Id} created an account");
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
			logger.LogInformation($"User {account.Id} logged out");
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
