using HandlebarsDotNet;

namespace BlogVb.Api.Services;

public interface ILayoutRenderer {
	string Render(string key, object? layoutData = default, object? bodyData = default);
	Task<string> RenderAsync(string key, object? layoutData = default, object? bodyData = default, CancellationToken cancellationToken = default);
}

public class LayoutRenderer : ILayoutRenderer {
	public string LayoutKey { get; set; } = "layout";
	private readonly IViewCache viewCache;

	public LayoutRenderer(IViewCache viewCache) {
		this.viewCache = viewCache;
	}

	public string Render(string key, object? layoutData = default, object? bodyData = default) {
		return RenderAsync(key, layoutData, bodyData).GetAwaiter().GetResult();
	}

	public async Task<string> RenderAsync(string key, object? layoutData = default, object? bodyData = default, CancellationToken cancellationToken = default) {
		var layout = Handlebars.Compile(await viewCache.GetViewAsync(LayoutKey, cancellationToken));
		var body = Handlebars.Compile(await viewCache.GetViewAsync(key, cancellationToken));

		bodyData ??= new { };
		layoutData ??= new { };

		DateTime now = DateTime.Now;
		object tools = new {
			year = now.Year,
			month = now.Month,
			day = now.Day,
			hour = now.Hour,
			minute = now.Minute,
			second = now.Second,

			date = now.Date,
			time = now.TimeOfDay,
		};

		object data = new {
			layout = layoutData,
			body = body(bodyData),

			//Extra tools that can be usefull
			tools
		};

		return layout(data);
	}

}
