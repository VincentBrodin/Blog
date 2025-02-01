using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Services;
using HandlebarsDotNet;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace BlogVb.Api.Controllers;
[Route("[controller]")]
[ApiController]
public class AccountController : ControllerBase {

	[HttpGet]
	[Route("login")]
	public async Task<IActionResult> GetLogin(ILayoutRenderer layoutRenderer) {
		return Content(await layoutRenderer.RenderAsync("pages/login"), Accepts.Html);
	}

	[HttpPost]
	[Route("login")]
	public async Task<IActionResult> PostLogin(IAccountService accountService, IViewCache viewCache, [FromForm] LoginAccount loginAccount) {
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
			Response.Headers.Append("HX-Redirect", "/");
			return Ok();
		}
	}


	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> GetCreate(ILayoutRenderer layoutRenderer) {
		return Content(await layoutRenderer.RenderAsync("pages/register"), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreate(IViewCache viewCache, IAccountService accountService, [FromForm] CreateAccount createAccount) {
		bool accountExists = accountService.GetByEmail(createAccount.Email) != null;
		object? validate = createAccount.Validate(accountExists);
		// If no errors and no accounts with that email
		if(validate == null) {
			Account account = createAccount.GetAccount();
			await accountService.Add(account);
			Response.Headers.Append("HX-Redirect", "/");
			return Ok();
		}
		else {
			string renderedHtml = Handlebars.Compile(await viewCache.GetViewAsync("pages/register"))(validate);
			return Content(renderedHtml, Accepts.Html);
		}
	}

}
