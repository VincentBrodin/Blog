﻿using BlogVb.Api.Models.Accounts;
using HandlebarsDotNet;

namespace BlogVb.Api.Services;

public interface ILayoutRenderer {
	string Render(string key, object? layoutData = default, object? bodyData = default);
	Task<string> RenderAsync(string key, object? layoutData = default, object? bodyData = default, CancellationToken cancellationToken = default);

	string RenderError(WebError error);
	Task<string> RenderErrorAsync(WebError error, CancellationToken cancellationToken = default);

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

		HttpContext? httpContext = httpContextAccessor.HttpContext;
		object data = new {
			account = httpContext == null ? null : cookieVault.Get<Account>(httpContext, "user"),
			layout = layoutData,
			body = body(bodyData),

			//Extra tools that can be usefull
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
			account = httpContext == null ? null : cookieVault.Get<Account>(httpContext, "user"),
			body = body(new {
				code = (int)error,
				message = Helper.GetErrorMessage(error),
			}),
			tools
		};

		return layout(data);
	}
}
