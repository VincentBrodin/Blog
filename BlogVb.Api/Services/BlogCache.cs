using BlogVb.Api.Models;
using System.Text;
using System.Text.Json;

namespace BlogVb.Api.Services;

public interface IBlogCache {
	Blog? GetBlog(string url, bool forceLoad = true);
	Blog[] GetAllBlogs();
	void CacheBlogs();
	Task CacheBlogsAsync(CancellationToken cancellationToken = default);
}

public class BlogCache : IBlogCache {
	private readonly Dictionary<string, Blog> blogs = [];
	private readonly string systemPath;
	private readonly bool preload;

	public BlogCache(string[] pathToCache, bool preload = false) {
		// Get the system path to our folder
		string globalPath = AppDomain.CurrentDomain.BaseDirectory;
		foreach(string path in pathToCache) {
			globalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}
		systemPath = globalPath;
		this.preload = preload;

		CacheBlogs();
	}

	public void CacheBlogs() {
		DateTime start = DateTime.Now;

		foreach(string path in Directory.GetFiles(systemPath, "*.md", SearchOption.AllDirectories)) {
			Blog blog = new(path);
			if(!blog.IsValid) {
				continue;
			}
			if(blogs.ContainsKey(blog.Url)) {
				Console.WriteLine($"(OBS!) Blog collision @ {path}");
				continue;
			}

			if(preload) {
				blog.Load();
			}

			blogs.Add(blog.Url, blog);
		}
		string preloadPrompt = preload ? "Preloaded" : "Not Preloaded";
		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(blogs));
		Console.WriteLine($"Cached all blogs [{preloadPrompt}] in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
	}

	public async Task CacheBlogsAsync(CancellationToken cancellationToken = default) {
		DateTime start = DateTime.Now;

		foreach(string path in Directory.GetFiles(systemPath, "*.md", SearchOption.AllDirectories)) {
			Blog blog = new(path);
			if(!blog.IsValid) {
				continue;
			}
			if(blogs.ContainsKey(blog.Url)) {
				Console.WriteLine($"(OBS!) Blog collision @ {path}");
				continue;
			}

			if(preload) {
				await blog.LoadAsync(cancellationToken);
			}

			blogs.Add(blog.Url, blog);
			Console.WriteLine($"Added {blog.Name}");
		}

		string preloadPrompt = preload ? "Preloaded" : "Not Preloaded";
		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(blogs));
		Console.WriteLine($"Cached all blogs [{preloadPrompt}] in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
	}

	public Blog[] GetAllBlogs() {
		return [.. blogs.Values];
	}

	public Blog? GetBlog(string url, bool forceLoad = true) {
		if(blogs.TryGetValue(url, out Blog? blog) && blog != null) {
			if(forceLoad && !blog.IsLoaded) {
				DateTime start = DateTime.Now;
				blog.Load();
				byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(blogs));
				Console.WriteLine($"Cached {blog.Name} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
			}
			return blog;
		}
		return null;
	}
}
