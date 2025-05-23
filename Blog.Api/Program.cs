using Blog.Api.Models.Accounts;
using Blog.Api.Services;
using Blog.Api.Tools;
using HandlebarsDotNet;
using Microsoft.EntityFrameworkCore;


namespace Blog.Api;

public static class Program
{
    public static double CacheDuration { get; private set; } = 3600;
    public static readonly string BlogDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "blogs");
    public static readonly string ContentImageDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "data", "images");

    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        CacheDuration = double.Parse(builder.Configuration["CacheDuration"]!);

        builder.Logging.ClearProviders();
        builder.Logging.AddConsole();

        // Add services to the container.
        builder.Services.AddControllers();
        builder.Services.AddHttpContextAccessor();

        Console.WriteLine(builder.Configuration["ConnectionStrings:SQLiteDefualt"]);
        builder.Services.AddDbContext<AppDbContext>(options => options.UseSqlite(builder.Configuration["ConnectionStrings:SQLiteDefualt"]));

        builder.Services.AddSingleton<IViewCache>(sp =>
        {
            ILogger<ViewCache> viewCacheLogger = sp.GetRequiredService<ILogger<ViewCache>>();
            ILogger<Cache<string>> cacheLogger = sp.GetRequiredService<ILogger<Cache<string>>>();

            ViewCache viewCache = new(viewCacheLogger, cacheLogger, ["views"]);


            Handlebars.RegisterHelper("role", (writer, options, context, arguments) =>
            {
                if (arguments.Length == 1 && arguments[0] is AccountModel.Roles role)
                {
                    if ((int)role >= 1)
                    {
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
        builder.Services.AddSingleton<IBlogCache>(sp =>
        {
            ILogger<BlogCache> blogCacheLogger = sp.GetRequiredService<ILogger<BlogCache>>();
            ILogger<Cache<Models.Blogs.BlogModel>> cacheLogger = sp.GetRequiredService<ILogger<Cache<Models.Blogs.BlogModel>>>();

            return new BlogCache(blogCacheLogger, cacheLogger, BlogDirectory);
        });
        builder.Services.AddSingleton<IImageCache>(sp =>
        {
            ILogger<ImageCache> imageCacheLogger = sp.GetRequiredService<ILogger<ImageCache>>();
            ILogger<Cache<byte[]>> cacheLogger = sp.GetRequiredService<ILogger<Cache<byte[]>>>();
            return new ImageCache(imageCacheLogger, cacheLogger);
        });
        builder.Services.AddSingleton<ICookieVault, CookieVault>();

        builder.Services.AddScoped<ILayoutRenderer, LayoutRenderer>();
        builder.Services.AddScoped<IAccountService, AccountService>();

        WebApplication app = builder.Build();

        // Configure the HTTP request pipeline.
        app.UseHttpsRedirection();
        app.UseAuthorization();
        app.MapControllers();
        app.UseStaticFiles();
        app.MapStaticAssets();

        app.Run();
    }
}

public static class Accepts
{
    public const string Html = "text/html";
    public const string Json = "application/json";
    public static bool IsHtml(string? accepts)
    {
        if (accepts == null) return false; // Defualts back to json
        return accepts == Html;
    }

    public static bool IsJson(string? accepts)
    {
        if (accepts == null) return true; // Defaults back to json
        return accepts == Json;
    }
}
