using Blog.Api.Tools;

namespace Blog.Api.Services;

public interface IViewCache {
	string GetView(string key);
	Task<string> GetViewAsync(string key, CancellationToken cancellationToken = default);
}
public class ViewCache : IViewCache, IAsyncDisposable {
	private readonly string systemPath;
	private readonly string filter;

	private readonly ILogger<ViewCache> logger;
	private readonly Cache<string> cache;

	public ViewCache(ILogger<ViewCache> viewCacheLogger, ILogger<Cache<string>> cacheLogger, string[] pathToCache, string filter = "*.*") {
		logger = viewCacheLogger;

		// Get the system path to our folder
		string globalPath = AppDomain.CurrentDomain.BaseDirectory;
		foreach(string path in pathToCache) {
			globalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}
		systemPath = globalPath;
		this.filter = filter;

		cache = new Cache<string>(Program.CacheDuration, cacheLogger);
	}


	public string GetView(string key) {
		return GetViewAsync(key).GetAwaiter().GetResult();
	}

	public async Task<string> GetViewAsync(string key, CancellationToken cancellationToken = default) {
		string? value = cache.Get(key);
		return value ?? await CacheViewAsync(key, cancellationToken);
	}

	private string CacheView(string key) {
		return CacheViewAsync(key).GetAwaiter().GetResult();
	}

	private async Task<string> CacheViewAsync(string key, CancellationToken cancellationToken = default) {
		DateTime start = DateTime.Now;
		foreach(string filePath in Directory.GetFiles(systemPath, filter, SearchOption.AllDirectories)) {
			string name = GetTemplateName(systemPath, filePath);
			if(name != key) {
				continue;
			}

			if(cache.Get(name) != null) {
				logger.LogError($"Tried to double allocate {name}");
			}
			string content = await File.ReadAllTextAsync(filePath, cancellationToken);

			cache.Add(name, content);
		}
		logger.LogInformation($"Cached {key} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms");
		string? value = cache.Get(key);

		if(value != null) {
			return value;
		}
		else {
			logger.LogError($"Could not get view {key} after cacheing");
			return "";
		}
	}

	private string GetTemplateName(string rootPath, string filePath) {
		string relativePath = Path.GetRelativePath(rootPath, filePath);
		string withoutExtension = Path.ChangeExtension(relativePath, null);
		return withoutExtension.Replace(Path.DirectorySeparatorChar, '/');
	}

	public async ValueTask DisposeAsync() {
		await cache.DisposeAsync();
	}

}
