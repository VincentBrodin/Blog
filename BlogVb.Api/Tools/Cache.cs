using System.Collections.Concurrent;

namespace BlogVb.Api.Tools;

public class Cache<T> : IAsyncDisposable {
	public class CacheValue {
		public string Key { get; set; }
		public T Value { get; set; }
		public DateTime CreationTime { get; set; }
		public DateTime ExperationTime { get; set; }

		public bool HasExpired => DateTime.Now > ExperationTime;

		public CacheValue(string key, T value, double duration) {
			Key = key;
			Value = value;
			CreationTime = DateTime.Now;
			ExperationTime = DateTime.Now.AddSeconds(duration);
		}
	}

	public class Worker : BackgroundService {
		private readonly int delayTime;
		private readonly Cache<T> cache;
		public Worker(Cache<T> cache, double experationTime) {
			this.cache = cache;
			delayTime = (int)Math.Round(experationTime * 1.5);
		}
		protected override async Task ExecuteAsync(CancellationToken stoppingToken) {
			while(!stoppingToken.IsCancellationRequested) {
				await Task.Delay(delayTime * 1000, stoppingToken);
				cache.Prune();
			}
		}
	}


	public double ExperationTime { get; init; } = 3600;
	//opublic ConDictionary<string, T>
	private readonly ConcurrentDictionary<string, CacheValue> cachedValues = [];
	private readonly ConcurrentQueue<string> cachedQueue = [];
	private Worker? worker;
	private readonly CancellationToken cancellationToken;
	private readonly ILogger<Cache<T>> logger;

	public Cache(double experationTime, ILogger<Cache<T>> logger, CancellationToken cancellationToken = default) {
		ExperationTime = experationTime;
		this.logger = logger;
		this.cancellationToken = cancellationToken;
	}


	public bool Add(string key, T value) {
		CacheValue cacheValue = new(key, value, ExperationTime);
		if(cachedValues.TryAdd(key, cacheValue)) {
			cachedQueue.Enqueue(key);

			if(worker == null) {
				worker = new Worker(this, ExperationTime);
				worker.StartAsync(cancellationToken).GetAwaiter().GetResult();
			}

			logger.LogTrace($"{key} added to cache");
			return true;
		}
		return false;
	}

	public T? Get(string key) {
		cachedValues.TryGetValue(key, out CacheValue? cacheValue);
		return cacheValue == null ? default : cacheValue.Value;
	}

	public T? Remove(string key) {
		cachedValues.TryRemove(key, out CacheValue? cacheValue);
		return cacheValue == null ? default : cacheValue.Value;
	}

	private void Prune() {
		logger.LogInformation("Starting pruning");
		int prunes = 0;
		while(cachedQueue.TryPeek(out string? tempKey) && tempKey != null) {
			if(cachedValues.TryGetValue(tempKey, out CacheValue? cacheValue) && cacheValue != null) {
				if(cacheValue.HasExpired && cachedQueue.TryDequeue(out string? key) && key != null) {
					cachedValues.TryRemove(key, out _);
					logger.LogTrace($"{tempKey} pruned");
					prunes++;
				}
				else {
					// Top of the queue is not expired yet 
					break;
				}
			}
			else {
				// Could not get value
				logger.LogWarning($"Prune could not get key {tempKey} might cause dead memory");
				cachedQueue.TryDequeue(out string? _);
				break;
			}
		}
		logger.LogInformation($"Pruned {prunes} items");
	}

	public async ValueTask DisposeAsync() {
		if(worker != null) {
			await worker.StopAsync(cancellationToken);
			worker.Dispose();
		}
	}
}