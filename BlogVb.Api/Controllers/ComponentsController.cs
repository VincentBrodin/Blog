using BlogVb.Api.Services;
using HandlebarsDotNet;
using Markdig;
using Microsoft.AspNetCore.Mvc;

namespace BlogVb.Api.Controllers;

[ApiController]
[Route("[controller]")]
public class ComponentsController : ControllerBase {


	private readonly ILogger<ComponentsController> _logger;
	public ComponentsController(ILogger<ComponentsController> logger) {
		_logger = logger;
	}


	[HttpGet]
	public IActionResult Get() {
		return Content("Hello World", Accepts.Html);
	}

	[HttpGet]
	[Route("blogs")]
	public IActionResult GetBlogs(IViewCache viewCache, IBlogCache blogCache) {
		string[] blogs = blogCache.GetAllBlogs().Select(b => b.Name).ToArray();
		string renderedHtml = Handlebars.Compile(viewCache.GetView("components/datalist"))(new { items = blogs });
		return Content(renderedHtml, Accepts.Html);
	}

	[HttpPost]
	[Route("mdtohtml")]
	public IActionResult PostMdToHtml([FromForm] string input) {
		return Content(Markdown.ToHtml(input), Accepts.Html);
	}

}
