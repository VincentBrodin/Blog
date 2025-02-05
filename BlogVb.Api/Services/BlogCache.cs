using BlogVb.Api.Models.Blogs;
using BlogVb.Api.Tools;

namespace BlogVb.Api.Services;

public interface IBlogCache {
	Blog? GetBlog(string url, bool load = false);
	Task<Blog?> GetBlogAsync(string url, bool load = false, CancellationToken cancellationToken = default);
	Task<List<Blog>> GetAllBlogsAsync(bool load = false, CancellationToken cancellationToken = default);
	Task<List<BlogForRendering>> GetAllBlogsForRenderingAsync(CancellationToken cancellationToken = default);
	Task CacheBlogAsync(Blog blog, CancellationToken cancellationToken = default);

}

public class BlogCache : IBlogCache, IAsyncDisposable {
	private readonly Dictionary<string, string> blogsPath = [];
	private readonly string systemPath;

	private readonly ILogger<BlogCache> logger;
	private readonly Cache<Blog> cache;

	public BlogCache(ILogger<BlogCache> blogCacheLogger, ILogger<Cache<Blog>> cacheLogger, string[] pathToCache) {
		// Get the system path to our folder
		string globalPath = AppDomain.CurrentDomain.BaseDirectory;
		foreach(string path in pathToCache) {
			globalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}
		systemPath = globalPath;

		foreach(string path in Directory.GetFiles(systemPath, "*.md", SearchOption.AllDirectories)) {
			Blog blog = new(path);
			blogsPath.Add(blog.Url, blog.ContentPath);
		}

		logger = blogCacheLogger;
		cache = new Cache<Blog>(Program.CacheDuration, cacheLogger);
	}

	public async Task CacheBlogAsync(Blog blog, CancellationToken cancellationToken = default) {
		if(!blog.HasMeta) {
			await blog.LoadMetaAsync(cancellationToken);
		}
		if(blogsPath.ContainsKey(blog.Url)) {
			logger.LogError($"(OBS!) Blog collision @ {blog.ContentPath}");
			return;
		}
		else {
			blogsPath.Add(blog.Url, blog.ContentPath);
		}

		cache.Add(blog.Url, blog);
	}


	public async Task<List<Blog>> GetAllBlogsAsync(bool load = false, CancellationToken cancellationToken = default) {
		List<Blog> list = [];
		foreach(string key in blogsPath.Keys) {
			Blog? blog = await GetBlogAsync(key, load, cancellationToken);
			if(blog == null) {
				continue;
			}
			list.Add(blog);
		}

		return list.OrderByDescending(b => b.CreatedAt).ToList();
	}

	public async Task<List<BlogForRendering>> GetAllBlogsForRenderingAsync(CancellationToken cancellationToken = default) {
		return (await GetAllBlogsAsync(false, cancellationToken)).ConvertAll(b => new BlogForRendering(b));
	}


	public Blog? GetBlog(string url, bool load = false) {
		return GetBlogAsync(url, load).GetAwaiter().GetResult();
	}

	public async Task<Blog?> GetBlogAsync(string url, bool load = false, CancellationToken cancellationToken = default) {
		DateTime start = DateTime.Now;

		Blog? blog = cache.Get(url);
		// We have not cached
		if(blog == null) {
			if(blogsPath.TryGetValue(url, out string? path) && path != null) {
				blog = new(path);
				if(!await blog.LoadMetaAsync(cancellationToken)) {
					logger.LogError($"Could not load meta for {url}");
					return null;
				}
				cache.Add(url, blog);
			}
			else {
				logger.LogError($"Missing path to {url}");
			}
		}

		if(blog == null) {
			logger.LogError($"Could not get view {url} after cacheing");
		}
		else {
			if(!blog.IsLoaded && load) {
				await blog.LoadContentAsync();
			}

			logger.LogInformation($"Cached {blog.Name}/{blog.Url} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms");
		}

		return blog;
	}

	public async ValueTask DisposeAsync() {
		await cache.DisposeAsync();
	}
}
