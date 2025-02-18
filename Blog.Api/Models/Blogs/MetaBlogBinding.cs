using Blog.Api.Tools;
using System.Text.Json.Serialization;

namespace Blog.Api.Models.Blogs;

public class MetaBlogBinding {
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;
	[JsonPropertyName("author")]
	public string Author { get; set; } = string.Empty;

	[JsonPropertyName("headerName")]
	public string HeaderName { get; set; } = string.Empty;
	[JsonPropertyName("createdAt")]
	public DateTime CreatedAt { get; set; }

	[JsonPropertyName("lastChangeAt")]
	public DateTime LastChangeAt { get; set; }

	[JsonPropertyName("readTimeSec")]
	public int ReadTimeSec { get; set; }
	[JsonPropertyName("readTimeMin")]
	public int ReadTimeMin { get; set; }


	public MetaBlogBinding() { }

	public MetaBlogBinding(CreateBlog createBlog) {
		Name = createBlog.Name;
		Description = createBlog.Description;
		(ReadTimeMin, ReadTimeSec) = Helper.CalculateReadTime(createBlog.Content);
	}

	public MetaBlogBinding(EditBlog editBlog) {
		Name = editBlog.Name;
		Description = editBlog.Description;
		(ReadTimeMin, ReadTimeSec) = Helper.CalculateReadTime(editBlog.Content);
	}

}

