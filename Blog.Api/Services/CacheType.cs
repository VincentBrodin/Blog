namespace Blog.Api.Services;

/// <summary>
/// Single mode caches a file when it is first requested, this speeds up the launch time of the application, but slows down the first calls.
/// Bunch grabs all of the files in to memory when it is first used, this slows down the startup but makes every request after blazingly fast.
/// </summary>
public enum CacheType {
	Single,
	Bunch
}
