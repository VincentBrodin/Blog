using Blog.Api.Tools;

namespace Blog.Api.Services;

public interface IImageCache {
	byte[]? GetImage(string path);
	Task<byte[]?> GetImageAsync(string path, CancellationToken cancellationToken = default);
}

public class ImageCache : IImageCache, IAsyncDisposable {
	private readonly Cache<byte[]> cache;
	private readonly ILogger<ImageCache> logger;

	public ImageCache(ILogger<ImageCache> imageCacheLogger, ILogger<Cache<byte[]>> cacheLogger) {
		logger = imageCacheLogger;
		cache = new Cache<byte[]>(Program.CacheDuration, cacheLogger);
	}

	public byte[]? GetImage(string path) {
		return GetImageAsync(path).GetAwaiter().GetResult();
	}

	public async Task<byte[]?> GetImageAsync(string path, CancellationToken cancellationToken = default) {
		return cache.Get(path) ?? await CacheImageAsync(path);
	}

	private async Task<byte[]?> CacheImageAsync(string path, CancellationToken cancellationToken = default) {
		DateTime start = DateTime.Now;

		if(!File.Exists(path)) {
			logger.LogError($"Could not find any file @ {path}");
			return null;
		}

		byte[] image = await File.ReadAllBytesAsync(path, cancellationToken);
		cache.Add(path, image);
		logger.LogInformation($"Cached {path} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms");
		byte[]? value = cache.Get(path);

		if(value != null) {
			return value;
		}
		else {
			logger.LogError($"Could not get image @ {path} after cacheing");
			return null; 
		}
	}


	public async ValueTask DisposeAsync() {
		await cache.DisposeAsync();
	}

}
