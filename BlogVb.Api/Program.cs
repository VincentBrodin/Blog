using BlogVb.Api.Services;
using HandlebarsDotNet;

namespace BlogVb.Api {
	public static class Program {
		public static void Main(string[] args) {
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();

			builder.Services.AddSingleton<IViewCache>(_ => {
				ViewCache viewCache = new(["views"], cacheType: CacheType.Single);
				Handlebars.RegisterTemplate("forecast", viewCache.GetView("components/forecast"));
				Handlebars.RegisterTemplate("nav", viewCache.GetView("components/nav"));
				Handlebars.RegisterTemplate("footer", viewCache.GetView("components/footer"));
				Handlebars.RegisterTemplate("blog", viewCache.GetView("components/blog"));
				return viewCache;
			});
			builder.Services.AddSingleton<IBlogCache>(_ => new BlogCache(["blogs"]));
			builder.Services.AddScoped<ILayoutRenderer, LayoutRenderer>();

			WebApplication app = builder.Build();

			// Configure the HTTP request pipeline.

			app.UseAuthorization();
			app.MapControllers();
			app.UseStaticFiles();

			app.Run();
		}
	}

	public static class Accepts {
		public const string Html = "text/html";
		public const string Json = "application/json";
		public static bool IsHtml(string? accepts) {
			if(accepts == null) return false; // Defualts back to json
			return accepts == Html;
		}

		public static bool IsJson(string? accepts) {
			if(accepts == null) return true; // Defaults back to json
			return accepts == Json;
		}
	}
}
