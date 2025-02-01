using System.Text.RegularExpressions;
using System.Net.Mail;

namespace BlogVb.Api;

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
}
