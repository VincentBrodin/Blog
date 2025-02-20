using System.Text.Json.Serialization;

namespace Blog.Api.Models.Images;

public class AddImage {
	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;
	[JsonPropertyName("content")]
	public IFormFile? Content { get; set; }
}
