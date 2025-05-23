﻿namespace Blog.Api.Models.Blogs;

public class BlogForRendering
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; private set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string HeaderName { get; set; } = string.Empty;
    public string CreatedAt { get; set; } = string.Empty;
    public string LastChangeAt { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int ReadTimeSec { get; set; }
    public int ReadTimeMin { get; set; }


    public BlogForRendering(BlogModel blog, bool includeContent = false)
    {
        Name = blog.Name;
        Url = blog.Url;
        Description = blog.Description;
        Author = blog.Author;
        HeaderName = blog.HeaderName;
        CreatedAt = blog.CreatedAt.ToString("dd MMM, yyyy @ HH:mm");
        LastChangeAt = blog.LastChangeAt.ToString("dd MMM, yyyy @ HH:mm");

        ReadTimeMin = blog.ReadTimeMin;
        ReadTimeSec = blog.ReadTimeSec;


        if (includeContent)
        {
            Content = blog.Content;
        }
    }
}

