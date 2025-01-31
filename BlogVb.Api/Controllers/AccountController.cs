using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Services;
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

	[HttpGet]
	[Route("create")]
	public async Task<IActionResult> GetCreate(ILayoutRenderer layoutRenderer) {
		return Content(await layoutRenderer.RenderAsync("pages/register"), Accepts.Html);
	}

	[HttpPost]
	[Route("create")]
	public async Task<IActionResult> PostCreate([FromForm] CreateAccount createAccount) {
		Account account = new(createAccount);
		return Content(JsonSerializer.Serialize(account), Accepts.Json);
	}

}
