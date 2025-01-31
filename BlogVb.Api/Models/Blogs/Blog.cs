using System.Text.Json;
using System.Web;

namespace BlogVb.Api.Models.Blogs;
public class Blog {
	public string Name { get; set; } = string.Empty;
	public string Url { get; } = string.Empty;
	public string Description { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; }
	public DateTime LastChangeAt { get; set; }


	public string Content { get; set; } = string.Empty;
	public int ReadTimeMin { get; set; }
	public int ReadTimeSec { get; set; }


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
			MetaBlogBinding? binding = JsonSerializer.Deserialize<MetaBlogBinding>(File.ReadAllText(MetaPath));
			if(binding == null) {
				Console.WriteLine($"Problem loading meta data: Could not parse json");
				return;
			}
			Name = binding.Name;
			Url = HttpUtility.UrlEncode(Helper.MakeFileSafe(Name));
			Description = binding.Description;
			CreatedAt = binding.CreatedAt;
			LastChangeAt = binding.LastChangeAt;
			ReadTimeMin = binding.ReadTimeMin;
			ReadTimeSec = binding.ReadTimeSec;
		}
		catch(Exception exception) {
			Console.WriteLine($"Problem loading meta data: {exception.Message}");
			return;
		}

		IsValid = true;
	}

	#region Generate New Blog
	public static Blog GenerateNewBlog(BlogFromPost blogFromPost) {
		return GenerateNewBlogAsync(blogFromPost).GetAwaiter().GetResult();
	}

	public static async Task<Blog> GenerateNewBlogAsync(BlogFromPost blogFromPost, CancellationToken cancellationToken = default) {
		string safeName = Helper.MakeFileSafe(blogFromPost.Name);
		safeName += ".md";
		string contentPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "blogs", safeName);
		string metaPath = contentPath + ".json";

		await File.WriteAllTextAsync(contentPath, blogFromPost.Content, cancellationToken);

		MetaBlogBinding bindings = new(blogFromPost) {
			CreatedAt = DateTime.Now,
			LastChangeAt = DateTime.Now,
		};
		string bindingsJson = JsonSerializer.Serialize(bindings);
		await File.WriteAllTextAsync(metaPath, bindingsJson, cancellationToken);

		// Makes sure that we don't need to load the blog twice saves us some compute :)
		return new Blog(contentPath) {
			Content = blogFromPost.Content,
			IsLoaded = true
		};
	}
	#endregion

	#region Load
	public void Load() {
		LoadAsync().GetAwaiter().GetResult();
	}

	public async Task LoadAsync(CancellationToken cancellationToken = default) {
		if(IsLoaded) {
			return;
		}
		if(!IsValid) {
			Console.WriteLine("Blog is not valid can't load");
		}

		try {
			Content = await File.ReadAllTextAsync(ContentPath, cancellationToken);
			IsLoaded = true;
		}
		catch(Exception exception) {
			Console.WriteLine($"Could not load blog {ContentPath} beacuse", exception.Message);
		}
	}
	#endregion

	public void Update() {

	}
}
