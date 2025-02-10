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


	[HttpPost]
	[Route("mdtohtml")]
	public IActionResult PostMdToHtml([FromForm] string content) {
		MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
			.UseAutoIdentifiers()
			.Build();
		return Content(Markdown.ToHtml(content, pipeline), Accepts.Html);
	}

	[HttpGet]
	[Route("blogs")]
	public IActionResult GetBlogs([FromQuery] int page) {
		return Content(page.ToString(), Accepts.Html);
	}


}
