using BlogVb.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;
[ApiController]
[Route("[controller]")]
public class AdminController : ControllerBase {

	[HttpGet]
	[Route("create")]
	public IActionResult GetCreate(ILayoutRenderer layoutRenderer) {
		return Content(layoutRenderer.Render("pages/create"), Accepts.Html);
	}

}
