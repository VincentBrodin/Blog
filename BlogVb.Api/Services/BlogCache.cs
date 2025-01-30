using BlogVb.Api.Models;
using System.Text;
using System.Text.Json;

namespace BlogVb.Api.Services;

public interface IBlogCache {
	Blog? GetBlog(string url, bool forceLoad = true);
	Task<Blog?> GetBlogAsync(string url, bool forceLoad = true, CancellationToken cancellationToken = default);
	Blog[] GetAllBlogs();
	BlogForRendering[] GetAllBlogsForRendering();
	void CacheBlogs();
	Task CacheBlogsAsync(CancellationToken cancellationToken = default);

	void CacheBlog(Blog blog);
	Task CacheBlogAsync(Blog blog, CancellationToken cancellationToken = default);

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
		CacheBlogsAsync().GetAwaiter().GetResult();
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


	public void CacheBlog(Blog blog) {
		CacheBlogAsync(blog).GetAwaiter().GetResult();
	}

	public async Task CacheBlogAsync(Blog blog, CancellationToken cancellationToken = default) {
		DateTime start = DateTime.Now;

		if(blogs.ContainsKey(blog.Url)) {
			Console.WriteLine($"(OBS!) Blog collision @ {blog.ContentPath}");
			return;
		}

		if(preload) {
			await blog.LoadAsync(cancellationToken);
		}

		blogs.Add(blog.Url, blog);

		string preloadPrompt = preload ? "Preloaded" : "Not Preloaded";
		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(blogs));
		Console.WriteLine($"Cached {blog.Name}  [{preloadPrompt}] in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
	}



	public Blog[] GetAllBlogs() {
		return blogs.Values
			.OrderByDescending(b => b.CreatedAt).ToArray();
	}

	public BlogForRendering[] GetAllBlogsForRendering() {
		return blogs.Values
			.OrderByDescending(b => b.CreatedAt)
			.Aggregate(new List<BlogForRendering>(), (list, b) => {
				list.Add(new BlogForRendering(b));
				return list;
			}).ToArray();
	}


	public Blog? GetBlog(string url, bool forceLoad = true) {
		return GetBlogAsync(url, forceLoad).GetAwaiter().GetResult();
	}

	public async Task<Blog?> GetBlogAsync(string url, bool forceLoad = true, CancellationToken cancellationToken = default) {
		if(blogs.TryGetValue(url, out Blog? blog) && blog != null) {
			if(blog.IsLoaded) {
				return blog;
			}

			if(forceLoad) {
				DateTime start = DateTime.Now;
				await blog.LoadAsync(cancellationToken);
				byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(blogs));
				Console.WriteLine($"Cached {blog.Name} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
			}

			return blog;
		}
		return null;
	}

}
