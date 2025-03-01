using System.Text.Json.Serialization;

namespace Blog.Api.Models.Blogs;

public class EditBlog {

	public EditBlog() { }

	public EditBlog(BlogModel blog) {
		Name = blog.Name;
		Url = blog.Url;
		Description = blog.Description;
		Content = blog.Content;
	}

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;
	[JsonPropertyName("url")]
	public string Url { get; set; } = string.Empty;
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("content")]
	public string Content { get; set; } = string.Empty;

	[JsonPropertyName("header")]
	public IFormFile? Header { get; set; }
}
