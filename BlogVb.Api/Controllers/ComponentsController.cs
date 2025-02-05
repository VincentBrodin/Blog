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
	public async Task<IActionResult> GetBlogsAsync(IViewCache viewCache, IBlogCache blogCache) {
		string[] blogs = (await blogCache.GetAllBlogsAsync()).Select(b => b.Name).ToArray();
		string renderedHtml = Handlebars.Compile(await viewCache.GetViewAsync("components/datalist"))(new { items = blogs });
		return Content(renderedHtml, Accepts.Html);
	}

	[HttpPost]
	[Route("mdtohtml")]
	public IActionResult PostMdToHtml([FromForm] string content) {
		MarkdownPipeline pipeline = new MarkdownPipelineBuilder()
			.UseAutoIdentifiers()
			.Build();
		return Content(Markdown.ToHtml(content, pipeline), Accepts.Html);
	}

}
