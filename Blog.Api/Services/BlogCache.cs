using Blog.Api.Models.Blogs;
using Blog.Api.Tools;

namespace Blog.Api.Services;

public interface IBlogCache
{
    BlogModel? GetBlog(string url, bool load = false);
    Task<BlogModel?> GetBlogAsync(string url, bool load = false, CancellationToken cancellationToken = default);
    Task<List<BlogModel>> GetAllBlogsAsync(bool load = false, CancellationToken cancellationToken = default);
    Task<List<BlogForRendering>> GetAllBlogsForRenderingAsync(CancellationToken cancellationToken = default);
    Task<List<BlogForRendering>> RangeBlogsForRenderingAsync(int start, int end, CancellationToken cancellationToken = default);
    Task CacheBlogAsync(BlogModel blog, CancellationToken cancellationToken = default);
    void Delete(BlogModel blog);

}

public class BlogCache : IBlogCache, IAsyncDisposable
{
    private readonly Dictionary<string, string> blogsPath = [];
    private readonly string systemPath;

    private readonly ILogger<BlogCache> logger;
    private readonly Cache<BlogModel> cache;

    public BlogCache(ILogger<BlogCache> blogCacheLogger, ILogger<Cache<BlogModel>> cacheLogger, string systemPath)
    {
        logger = blogCacheLogger;
        cache = new Cache<BlogModel>(Program.CacheDuration, cacheLogger);

        // Get the system path to our folder
        this.systemPath = systemPath;
        if (!Directory.Exists(systemPath))
            Directory.CreateDirectory(systemPath);

        foreach (string path in Directory.GetFiles(systemPath, "*.md", SearchOption.AllDirectories))
        {
            Models.Blogs.BlogModel blog = new(path);
            blogsPath.Add(blog.Url, blog.ContentPath);
        }

    }

    public async Task CacheBlogAsync(BlogModel blog, CancellationToken cancellationToken = default)
    {
        if (!blog.HasMeta)
        {
            await blog.LoadMetaAsync(cancellationToken);
        }
        if (blogsPath.ContainsKey(blog.Url))
        {
            logger.LogError($"(OBS!) Blog collision @ {blog.ContentPath}");
            return;
        }
        else
        {
            blogsPath.Add(blog.Url, blog.ContentPath);
        }

        cache.Add(blog.Url, blog);
    }


    public async Task<List<BlogModel>> GetAllBlogsAsync(bool load = false, CancellationToken cancellationToken = default)
    {
        List<BlogModel> list = [];
        foreach (string key in blogsPath.Keys)
        {
            BlogModel? blog = await GetBlogAsync(key, load, cancellationToken);
            if (blog == null)
            {
                continue;
            }
            list.Add(blog);
        }

        return list.OrderByDescending(b => b.CreatedAt).ToList();
    }

    public async Task<List<BlogForRendering>> GetAllBlogsForRenderingAsync(CancellationToken cancellationToken = default)
    {
        return (await GetAllBlogsAsync(false, cancellationToken)).ConvertAll(b => new BlogForRendering(b));
    }
    public async Task<List<BlogForRendering>> RangeBlogsForRenderingAsync(int start, int end, CancellationToken cancellationToken = default)
    {
        List<BlogModel> blogs = await GetAllBlogsAsync(false, cancellationToken);
        if (end > blogs.Count)
        {
            end = blogs.Count;
        }

        List<BlogForRendering> list = [];
        for (int i = start; i < end; i++)
        {
            list.Add(new BlogForRendering(blogs[i]));
        }
        return list;
    }



    public BlogModel? GetBlog(string url, bool load = false)
    {
        return GetBlogAsync(url, load).GetAwaiter().GetResult();
    }

    public async Task<BlogModel?> GetBlogAsync(string url, bool load = false, CancellationToken cancellationToken = default)
    {
        BlogModel? blog = cache.Get(url);
        // We have not cached
        if (blog == null)
        {
            DateTime start = DateTime.Now;
            if (blogsPath.TryGetValue(url, out string? path) && path != null)
            {
                blog = new(path);
                if (!await blog.LoadMetaAsync(cancellationToken))
                {
                    logger.LogError($"Could not load meta for {url}");
                    return null;
                }
                cache.Add(url, blog);
            }
            else
            {
                logger.LogError($"Missing path to {url}");
            }

            if (blog == null)
            {
                logger.LogError($"Could not get view {url} after cacheing");
            }
            else
            {

                logger.LogInformation($"Cached {blog.Name}/{blog.Url} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms");
            }
        }
        if (blog == null)
        {
            logger.LogError($"Could not get view {url} after cacheing");
        }
        else if (!blog.IsLoaded && load)
        {
            await blog.LoadContentAsync(cancellationToken);
        }
        return blog;

    }

    public void Delete(BlogModel blog)
    {
        logger.LogInformation($"Deleting {blog.Name} from cache");
        cache.Remove(blog.Url);
        blogsPath.Remove(blog.Url);
        blog.Delete();
    }

    public async ValueTask DisposeAsync()
    {
        await cache.DisposeAsync();
    }
}
