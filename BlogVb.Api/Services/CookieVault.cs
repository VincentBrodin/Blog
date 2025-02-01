namespace BlogVb.Api.Services;

public interface ICookieVault {
	public T? Get<T>(HttpContext context, string name);
	public void Set<T>(HttpContext context, string name, T value);
	public void Remove(HttpContext context, string name);
}

public class CookieVault : ICookieVault {
	// 1 hour
	public const int CookieDuration = 3600;
	private class StoredCookie {
		public Guid Id { get; set; }
		public object? Value { get; set; }
		public DateTimeOffset ExpireTime { get; set; }

		public bool IsExpired => DateTimeOffset.UtcNow > ExpireTime;
	}

	private readonly Dictionary<Guid, StoredCookie> cookies = [];
	private readonly Queue<Guid> queue = [];

	public T? Get<T>(HttpContext context, string name) {
		while(queue.Count > 0) {
			if(cookies.TryGetValue(queue.Peek(), out StoredCookie? cookie) && cookie != null) {
				if(cookie.IsExpired) {
					cookies.Remove(cookie.Id);
					queue.Dequeue();
				}
			}
		}

		// If cookie exists
		if(context.Request.Cookies.TryGetValue(name, out string? idString) && idString != null) {
			Guid id = Guid.Parse(idString);
			if(cookies.TryGetValue(id, out StoredCookie? cookie) && cookie != null) {
				if(cookie.IsExpired) {
					cookies.Remove(id);
					context.Response.Cookies.Delete(name);
					return default;
				}
				return cookie.Value == null ? default : (T)cookie.Value;
			}
			else {
				context.Response.Cookies.Delete(name);
			}

		}
		return default;
	}

	public void Set<T>(HttpContext context, string name, T value) {
		StoredCookie cookie = new() {
			Id = Guid.NewGuid(),
			Value = value,
			ExpireTime = DateTimeOffset.UtcNow.AddSeconds(CookieDuration)
		};
		cookies.Add(cookie.Id, cookie);
		context.Response.Cookies.Append(name, cookie.Id.ToString(), new CookieOptions { Expires = cookie.ExpireTime });
	}

	public void Remove(HttpContext context, string name) {
		if(context.Request.Cookies.TryGetValue(name, out string? idString) && idString != null) {
			Guid id = Guid.Parse(idString);
			cookies.Remove(id);
			context.Response.Cookies.Delete(name);
		}
	}
}
