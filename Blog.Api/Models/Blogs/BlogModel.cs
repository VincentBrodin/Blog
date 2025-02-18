using Blog.Api.Tools;
using System.Text.Json;
using System.Web;

namespace Blog.Api.Models.Blogs;
public class BlogModel {
	public string Name { get; set; } = string.Empty;
	public string Url { get; } = string.Empty;
	public string Description { get; set; } = string.Empty;
	public string Author { get; set; } = string.Empty;
	public string HeaderName { get; set; } = string.Empty;

	public DateTime CreatedAt { get; set; }
	public DateTime LastChangeAt { get; set; }


	public string Content { get; set; } = string.Empty;
	public int ReadTimeMin { get; set; }
	public int ReadTimeSec { get; set; }


	public string ContentPath { get; init; } = string.Empty;
	public string MetaPath { get; init; } = string.Empty;

	public bool HasMeta { get; private set; }
	public bool IsLoaded { get; private set; }


	public BlogModel(string contentPath) {
		ContentPath = contentPath;
		MetaPath = contentPath + ".json";
		string fileName = Path.GetFileNameWithoutExtension(ContentPath);
		Url = HttpUtility.UrlEncode(Helper.MakeSafe(fileName));
	}


	#region Generate New Blog
	public static BlogModel GenerateNewBlog(CreateBlog createBlog) {
		return GenerateNewBlogAsync(createBlog).GetAwaiter().GetResult();
	}

	public static async Task<BlogModel> GenerateNewBlogAsync(CreateBlog createBlog, string author = "John Doe", CancellationToken cancellationToken = default) {
		if(!Directory.Exists(Program.BlogDirectory))
			Directory.CreateDirectory(Program.BlogDirectory);

		string safeName = Helper.MakeSafe(createBlog.Name);
		string contentName = safeName + ".md";
		string contentPath = Path.Combine(Program.BlogDirectory, contentName);
		string metaPath = contentPath + ".json";

		await File.WriteAllTextAsync(contentPath, createBlog.Content, cancellationToken);

		MetaBlogBinding binding = new(createBlog) {
			Author = author,
			CreatedAt = DateTime.Now,
			LastChangeAt = DateTime.Now,
		};

		// Write image to disk and add section to meta
		if(createBlog.Header != null) {
			string imageName = Helper.MakeSafe(binding.Name) + Path.GetFileName(createBlog.Header.FileName);
			string imagePath = Path.Combine(Program.BlogDirectory, imageName);

			binding.HeaderName = imageName;

			await using Stream fileStream = new FileStream(imagePath, FileMode.Create);
			await createBlog.Header.CopyToAsync(fileStream, cancellationToken);
		}

		string bindingsJson = JsonSerializer.Serialize(binding);
		await File.WriteAllTextAsync(metaPath, bindingsJson, cancellationToken);

		// Makes sure that we don't need to load the blog twice saves us some compute :)
		BlogModel blog = new(contentPath);
		await blog.LoadMetaAsync(cancellationToken);
		return blog;
	}
	#endregion

	#region Load
	public void LoadContent() {
		LoadContentAsync().GetAwaiter().GetResult();
	}

	public async Task LoadContentAsync(CancellationToken cancellationToken = default) {
		if(IsLoaded) {
			return;
		}
		if(!HasMeta) {
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

	public async Task<bool> LoadMetaAsync(CancellationToken cancellationToken = default) {
		if(HasMeta) {
			return true;
		}
		if(!File.Exists(ContentPath) || !File.Exists(MetaPath)) {
			return false;
		}

		try {
			MetaBlogBinding? binding = JsonSerializer.Deserialize<MetaBlogBinding>(await File.ReadAllTextAsync(MetaPath, cancellationToken));
			if(binding == null) {
				return false;
			}
			Name = binding.Name;
			Description = binding.Description;
			Author = binding.Author;
			HeaderName = binding.HeaderName;
			CreatedAt = binding.CreatedAt;
			LastChangeAt = binding.LastChangeAt;
			ReadTimeMin = binding.ReadTimeMin;
			ReadTimeSec = binding.ReadTimeSec;
		}
		catch(Exception exception) {
			Console.WriteLine($"Problem loading meta data: {exception.Message}");
			return false;
		}
		HasMeta = true;
		return true;
	}

	#endregion

	public async Task UpdateAsync(EditBlog editBlog, CancellationToken cancellationToken = default) {
		await File.WriteAllTextAsync(ContentPath, editBlog.Content, cancellationToken);

		MetaBlogBinding? binding = JsonSerializer.Deserialize<MetaBlogBinding>(File.ReadAllText(MetaPath));

		if(binding == null) {
			Console.WriteLine($"Problem loading meta data: Could not parse json");
			return;
		}

		binding.Name = editBlog.Name;
		binding.Description = editBlog.Description;
		binding.LastChangeAt = DateTime.Now;
		(binding.ReadTimeMin, binding.ReadTimeSec) = Helper.CalculateReadTime(editBlog.Content);

		if(editBlog.Header != null) {
			string oldImagePath = Path.Combine(Program.BlogDirectory, binding.HeaderName);
			if(File.Exists(oldImagePath)) {
				File.Delete(oldImagePath);
			}

			binding.HeaderName = Helper.MakeSafe(binding.Name) + Path.GetFileName(editBlog.Header.FileName);

			string newImagePath = Path.Combine(Program.BlogDirectory, binding.HeaderName);
			await using Stream fileStream = new FileStream(newImagePath, FileMode.Create);
			await editBlog.Header.CopyToAsync(fileStream, cancellationToken);
		}

		string bindingsJson = JsonSerializer.Serialize(binding);
		await File.WriteAllTextAsync(MetaPath, bindingsJson, cancellationToken);

		Name = binding.Name;
		Description = binding.Description;
		HeaderName = binding.HeaderName;
		Content = editBlog.Content;
		LastChangeAt = binding.LastChangeAt;
		ReadTimeMin = binding.ReadTimeMin;
		ReadTimeSec = binding.ReadTimeSec;

		IsLoaded = true;
		HasMeta = true;
	}
}
