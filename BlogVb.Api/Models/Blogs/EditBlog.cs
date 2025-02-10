﻿using System.Text.Json.Serialization;

namespace BlogVb.Api.Models.Blogs;

public class EditBlog {

	public EditBlog() { }

	public EditBlog(Blog blog) {
		Name = blog.Name;
		Description = blog.Description;
		Content = blog.Content;
	}

	[JsonPropertyName("name")]
	public string Name { get; set; } = string.Empty;
	[JsonPropertyName("description")]
	public string Description { get; set; } = string.Empty;

	[JsonPropertyName("content")]
	public string Content { get; set; } = string.Empty;

	[JsonPropertyName("header")]
	public IFormFile? Header { get; set; }
}
