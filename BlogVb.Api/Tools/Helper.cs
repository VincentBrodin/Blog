using Microsoft.AspNetCore.StaticFiles;
using System.Dynamic;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace BlogVb.Api.Tools;
public enum WebError {
	BadRequest = 400,
	Unauthorized = 401,
	Forbidden = 403,
	NotFound = 404,
	MethodNotAllowed = 405,
	InternalServerError = 500,
	NotImplemented = 501,
	ServiceUnavailable = 503
}

public static class Helper {
	public static string FormatStorageSize(long bytes) {
		string[] sizes = { "B", "KB", "MB", "GB", "TB", "PB", "EB" };
		double len = bytes;
		int order = 0;

		while(len >= 1024 && order < sizes.Length - 1) {
			order++;
			len /= 1024;
		}

		return $"{len:0.##} {sizes[order]}";
	}

	public static (int, int) CalculateReadTime(string markdown) {
		markdown = Regex.Replace(markdown, @"(\*\*|__|\*|_|~~|`{1,3}|#{1,6}|\[.*?\]\(.*?\)|!\[.*?\]\(.*?\)|\>|-{3,}|={3,}|:.*?:|\|)", "");
		markdown = Regex.Replace(markdown, @"[^a-zA-Z0-9]", "");

		int characterCount = markdown.Length;
		double wordCount = characterCount / 5.1; // Avrage word length in english

		int readTimeMinutes = (int)Math.Floor(wordCount / 200);
		int readTimeSeconds = (int)Math.Round(wordCount % 200 * 60 / 200);

		return (readTimeMinutes, readTimeSeconds);
	}

	public static string MakeFileSafe(string input) {
		foreach(char c in Path.GetInvalidFileNameChars()) {
			input = input.Replace(c, '-');
		}
		return input;
	}

	public static bool IsValidEmail(string email) {
		var trimmedEmail = email.Trim();

		if(!trimmedEmail.EndsWith('.')) {
			try {
				var addr = new MailAddress(email);
				return addr.Address == trimmedEmail;
			}
			catch {
				return false;
			}
		}
		return false;
	}

	public static string GetErrorMessage(WebError error) {
		switch(error) {
			case WebError.BadRequest:
				return "Bad Request: The server could not understand the request due to invalid syntax.";
			case WebError.Unauthorized:
				return "Unauthorized: The client must authenticate itself to get the requested response.";
			case WebError.Forbidden:
				return "Forbidden: The client does not have access rights to the content.";
			case WebError.NotFound:
				return "Not Found: The server cannot find the requested resource.";
			case WebError.MethodNotAllowed:
				return "Method Not Allowed: The request method is known by the server but is not supported by the target resource.";
			case WebError.InternalServerError:
				return "Internal Server Error: The server has encountered a situation it doesn't know how to handle.";
			case WebError.NotImplemented:
				return "Not Implemented: The request method is not supported by the server.";
			case WebError.ServiceUnavailable:
				return "Service Unavailable: The server is not ready to handle the request.";
			default:
				return "Unknown error.";
		}
	}

	public static string GetFileContentType(string filePath) {
		FileExtensionContentTypeProvider provider = new();
		if(!provider.TryGetContentType(filePath, out string? contentType)) {
			contentType = "application/octet-stream";
		}

		return contentType;
	}
	public static ExpandoObject? ToExpandoObject(dynamic obj) {
		if(obj is ExpandoObject expando)
			return expando;

		var dictionary = new ExpandoObject() as IDictionary<string, object>;
		foreach(var property in obj.GetType().GetProperties()) {
			dictionary[property.Name] = property.GetValue(obj);
		}

		return dictionary as ExpandoObject;
	}

}
