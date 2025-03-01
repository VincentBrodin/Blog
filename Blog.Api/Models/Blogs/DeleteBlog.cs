using System.Text.Json.Serialization;

namespace Blog.Api.Models.Blogs;

public class DeleteBlog {

	public DeleteBlog() { }

	[JsonPropertyName("confirm")]
	public string Confirm { get; set; } = string.Empty;
}
