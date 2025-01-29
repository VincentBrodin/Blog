using System.Text;
using System.Text.Json;

namespace BlogVb.Api.Services;

public interface IViewCache {
	string GetView(string key);
	void CacheViews();
	Task CacheViewsAsync(CancellationToken cancellationToken = default);
}
public class ViewCache : IViewCache {
	public bool HotReload { get; set; }

	private readonly Dictionary<string, string> views = [];
	private readonly string systemPath;
	private readonly string filter;
	private bool hasCached;

	private readonly CacheType cacheType;

	public ViewCache(string[] pathToCache, string filter = "*.*", CacheType cacheType = CacheType.Bunch) {
		// Get the system path to our folder
		string globalPath = AppDomain.CurrentDomain.BaseDirectory;
		foreach(string path in pathToCache) {
			globalPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, path);
		}
		systemPath = globalPath;
		this.filter = filter;

		this.cacheType = cacheType;
		if(cacheType == CacheType.Bunch) {
			CacheViews();
		}
	}

	public void CacheViews() {
		if(hasCached) {
			return;
		}

		DateTime start = DateTime.Now;

		foreach(string filePath in Directory.GetFiles(systemPath, filter, SearchOption.AllDirectories)) {
			string name = GetTemplateName(systemPath, filePath);
			if(views.ContainsKey(name)) {
				Console.WriteLine($"(OBS!) File name collision @ {filePath}");
				continue;
			}
			string content = File.ReadAllText(filePath);
			views.Add(name, content);
			Console.WriteLine($"Added {name}");
		}
		hasCached = true;

		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(views));
		Console.WriteLine($"Cached all views in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
	}

	public async Task CacheViewsAsync(CancellationToken cancellationToken = default) {
		if(hasCached) {
			return;
		}

		DateTime start = DateTime.Now;

		foreach(string filePath in Directory.GetFiles(systemPath, filter, SearchOption.AllDirectories)) {
			string name = GetTemplateName(systemPath, filePath);
			if(views.ContainsKey(name)) {
				Console.WriteLine($"(OBS!) File name collision @ {filePath}");
				continue;
			}
			string content = await File.ReadAllTextAsync(filePath, cancellationToken);
			views.Add(name, content);
			Console.WriteLine($"Added {name}");
		}
		hasCached = true;

		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(views));
		Console.WriteLine($"Cached all views in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");
	}

	public string CacheSingleView(string key) {
		DateTime start = DateTime.Now;
		foreach(string filePath in Directory.GetFiles(systemPath, filter, SearchOption.AllDirectories)) {
			string name = GetTemplateName(systemPath, filePath);
			if(name != key) {
				continue;
			}

			if(views.ContainsKey(name)) {
				throw new Exception($"(OBS!) File name collision @ {filePath}");
			}
			string content = File.ReadAllText(filePath);

			if(HotReload) {
				Console.WriteLine("(OBS!) Hot reload is enabled");
				Console.WriteLine(content);
				return content;
			}

			views.Add(name, content);
		}
		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(views));
		Console.WriteLine($"Cached {key} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");

		return views[key];
	}

	public async Task<string> CacheSingleViewAsync(string key) {
		DateTime start = DateTime.Now;
		foreach(string filePath in Directory.GetFiles(systemPath, filter, SearchOption.AllDirectories)) {
			string name = GetTemplateName(systemPath, filePath);
			if(name != key) {
				continue;
			}

			if(views.ContainsKey(name)) {
				throw new Exception($"(OBS!) File name collision @ {filePath}");
			}
			string content = await File.ReadAllTextAsync(filePath);

			if(HotReload) {
				Console.WriteLine("(OBS!)Hot reload is enabled");
				Console.WriteLine(content);
				return content;
			}

			views.Add(name, content);
		}
		byte[] bytes = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(views));
		Console.WriteLine($"Cached {key} in {DateTime.Now.Subtract(start).TotalMilliseconds}ms [{Helper.FormatStorageSize(bytes.LongLength)}]");

		return views[key];
	}



	public string GetView(string key) {
		if(!hasCached && cacheType == CacheType.Bunch && !HotReload) {
			CacheViews();
		}

		if(views.TryGetValue(key, out string? content) && content != null) {
			return content;
		}
		// We could not find the view
		if(cacheType == CacheType.Single || HotReload) {
			return CacheSingleView(key);
		}
		throw new Exception($"View {key} does not exist");
	}

	private string GetTemplateName(string rootPath, string filePath) {
		string relativePath = Path.GetRelativePath(rootPath, filePath);
		string withoutExtension = Path.ChangeExtension(relativePath, null);
		return withoutExtension.Replace(Path.DirectorySeparatorChar, '/');
	}
}
