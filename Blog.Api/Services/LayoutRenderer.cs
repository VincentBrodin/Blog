using Blog.Api.Models.Accounts;
using Blog.Api.Tools;
using HandlebarsDotNet;

namespace Blog.Api.Services;

public interface ILayoutRenderer {
	string Render(string key, object? layoutData = default, object? bodyData = default);
	Task<string> RenderAsync(string key, object? layoutData = default, object? bodyData = default, CancellationToken cancellationToken = default);

	string RenderError(WebError error);
	Task<string> RenderErrorAsync(WebError error, CancellationToken cancellationToken = default);


	string RenderErrorClean(WebError error);
	Task<string> RenderErrorCleanAsync(WebError error, CancellationToken cancellationToken = default);

}

public class LayoutRenderer : ILayoutRenderer {
	public string LayoutKey { get; set; } = "layout";
	public string ErrorKey { get; set; } = "error";
	private readonly IViewCache viewCache;
	private readonly ICookieVault cookieVault;
	private readonly IHttpContextAccessor httpContextAccessor;

	public LayoutRenderer(IViewCache viewCache, ICookieVault cookieVault, IHttpContextAccessor httpContextAccessor) {
		this.viewCache = viewCache;
		this.cookieVault = cookieVault;
		this.httpContextAccessor = httpContextAccessor;
	}

	public string Render(string key, object? layoutData = default, object? bodyData = default) {
		return RenderAsync(key, layoutData, bodyData).GetAwaiter().GetResult();
	}

	public async Task<string> RenderAsync(string key, dynamic? layoutData = default, dynamic? bodyData = default, CancellationToken cancellationToken = default) {
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

		HttpContext? httpContext = httpContextAccessor.HttpContext;
		AccountModel? account = httpContext == null ? null : cookieVault.Get<AccountModel>(httpContext, "user");

		// Convert bodyData to ExpandoObject if needed
		var mergedBodyData = Helper.ToExpandoObject(bodyData);
		mergedBodyData.account = account; // Inject account into bodyData
		bodyData = mergedBodyData;

		object data = new {
			account,
			layout = layoutData,
			body = body(bodyData),
			tools
		};

		return layout(data);
	}

	public string RenderError(WebError error) {
		return RenderErrorAsync(error).GetAwaiter().GetResult();
	}

	public async Task<string> RenderErrorAsync(WebError error, CancellationToken cancellationToken = default) {
		var layout = Handlebars.Compile(await viewCache.GetViewAsync(LayoutKey, cancellationToken));
		var body = Handlebars.Compile(await viewCache.GetViewAsync(ErrorKey, cancellationToken));


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

		HttpContext? httpContext = httpContextAccessor.HttpContext;
		if(httpContext != null) {
			httpContext.Response.StatusCode = (int)error;
		}

		object data = new {
			account = httpContext == null ? null : cookieVault.Get<AccountModel>(httpContext, "user"),
			body = body(new {
				code = (int)error,
				message = Helper.GetErrorMessage(error),
			}),
			tools
		};

		return layout(data);
	}

	public string RenderErrorClean(WebError error) {
		return RenderErrorCleanAsync(error).GetAwaiter().GetResult();
	}

	public async Task<string> RenderErrorCleanAsync(WebError error, CancellationToken cancellationToken = default) {
		var body = Handlebars.Compile(await viewCache.GetViewAsync(ErrorKey, cancellationToken));

		HttpContext? httpContext = httpContextAccessor.HttpContext;
		if(httpContext != null) {
			httpContext.Response.StatusCode = (int)error;
		}

		return body(new {
			code = (int)error,
			message = Helper.GetErrorMessage(error),
		});
	}

}
