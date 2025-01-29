using System.Text.Json;
using System.Text.Json.Serialization;
using System.Web;

namespace BlogVb.Api.Models;

public struct MetaBlogBinding {
	[JsonPropertyName("name")]
	public string Name { get; set; }
	[JsonPropertyName("description")]
	public string Description { get; set; }
}

public class Blog {
	public string Name { get; set; } = string.Empty;
	public string Url { get; private set; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Content { get; set; } = string.Empty;
	public string ContentPath { get; init; } = string.Empty;
	public string MetaPath { get; init; } = string.Empty;
	public bool IsValid { get; }
	public bool IsLoaded { get; private set; }


	public Blog(string contentPath) {
		ContentPath = contentPath;
		MetaPath = contentPath + ".json";

		if(!File.Exists(ContentPath) || !File.Exists(MetaPath)) {
			Console.WriteLine($"Could create blog {contentPath} beacuse a path is missing.");
			return;
		}

		try {
			MetaBlogBinding binding = JsonSerializer.Deserialize<MetaBlogBinding>(File.ReadAllText(MetaPath));
			Name = binding.Name;
			Url = HttpUtility.UrlEncode(Name);
			Description = binding.Description;
		}
		catch(Exception exception) {
			Console.WriteLine($"Problem loading meta data: {exception.Message}");
			return;
		}

		IsValid = true;
	}

	public void Load() {
		if(IsLoaded) {
			return;
		}
		if(!IsValid) {
			Console.WriteLine("Blog is not valid can't load");
		}

		try {
			Content = File.ReadAllText(ContentPath);
			IsLoaded = true;
		}
		catch(Exception exception) {
			Console.WriteLine($"Could not load blog {ContentPath} beacuse", exception.Message);
		}
	}

	public async Task LoadAsync(CancellationToken cancellationToken = default) {
		if(IsLoaded) {
			return;
		}
		if(!IsValid) {
			Console.WriteLine("Blog is not valid can't load");
		}

		try {
			MetaBlogBinding binding = JsonSerializer.Deserialize<MetaBlogBinding>(await File.ReadAllTextAsync(MetaPath, cancellationToken));

			Name = binding.Name;
			Url = HttpUtility.UrlEncode(Name);
			Description = binding.Description;

			Content = await File.ReadAllTextAsync(ContentPath, cancellationToken);

			IsLoaded = true;
		}
		catch(Exception exception) {
			Console.WriteLine($"Could not load blog {ContentPath} beacuse", exception.Message);
		}
	}

	public void Update() {

	}
}
