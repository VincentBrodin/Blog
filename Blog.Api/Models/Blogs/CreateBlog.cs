﻿using System.Text.Json.Serialization;

namespace Blog.Api.Models.Blogs;

public class CreateBlog
{
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;

    [JsonPropertyName("header")]
    public IFormFile? Header { get; set; }
}
