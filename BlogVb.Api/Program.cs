using BlogVb.Api.Models.Accounts;
using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Services;
using BlogVb.Api.Tools;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;

namespace BlogVb.Api {
	public static class Program {
		public static double CacheDuration { get; private set; } = 3600;
		public static void Main(string[] args) {
			WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

			CacheDuration = double.Parse(builder.Configuration["CacheDuration"]!);

			builder.Logging.ClearProviders();
			builder.Logging.AddConsole();

			// Add services to the container.

			builder.Services.AddControllers();
			builder.Services.AddHttpContextAccessor();

			Console.WriteLine(builder.Configuration["ConnectionStrings:SQLiteDefualt"]);
			builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration["ConnectionStrings:SQLiteDefualt"]));

			builder.Services.AddSingleton<IViewCache>(sp => {
				ILogger<ViewCache> viewCacheLogger = sp.GetRequiredService<ILogger<ViewCache>>();
				ILogger<Cache<string>> cacheLogger = sp.GetRequiredService<ILogger<Cache<string>>>();

				ViewCache viewCache = new(viewCacheLogger, cacheLogger, ["views"]);


				Handlebars.RegisterHelper("role", (writer, options, context, arguments) => {
					if(arguments.Length == 1 && arguments[0] is Account.Roles role) {
						if((int)role >= 1) {
							options.Template(writer, context);
						}
					}
				});

				Handlebars.RegisterTemplate("forecast", viewCache.GetView("components/forecast"));
				Handlebars.RegisterTemplate("nav", viewCache.GetView("components/nav"));
				Handlebars.RegisterTemplate("footer", viewCache.GetView("components/footer"));
				Handlebars.RegisterTemplate("blog", viewCache.GetView("components/blog"));
				return viewCache;
			});
			builder.Services.AddSingleton<IBlogCache>(sp => {
				ILogger<BlogCache> blogCacheLogger = sp.GetRequiredService<ILogger<BlogCache>>();
				ILogger<Cache<Blog>> cacheLogger = sp.GetRequiredService<ILogger<Cache<Blog>>>();

				return new BlogCache(blogCacheLogger, cacheLogger, ["blogs"]);
			});
			builder.Services.AddSingleton<ICookieVault, CookieVault>();

			builder.Services.AddScoped<ILayoutRenderer, LayoutRenderer>();
			builder.Services.AddScoped<IAccountService, AccountService>();

			WebApplication app = builder.Build();

			// Configure the HTTP request pipeline.

			app.UseAuthorization();
			app.MapControllers();
			app.UseStaticFiles();
			app.MapStaticAssets();

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
