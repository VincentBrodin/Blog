using Blog.Api.Tools;

namespace Blog.Api.Models.Images;

public class ImageForRendering
{
    public string Name { get; set; } = string.Empty;
    public string Extension { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string WebPath { get; set; } = string.Empty;
    public string SystemPath { get; set; } = string.Empty;
    public string Size { get; set; } = string.Empty;

    public ImageForRendering(string path)
    {
        Name = Path.GetFileNameWithoutExtension(path);
        Extension = Path.GetExtension(path);
        FullName = Path.GetFileName(path);
        WebPath = $"/content/{FullName}";
        SystemPath = path;
        Size = Helper.FormatBytes(path);
    }
}
