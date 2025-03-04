namespace Blog.Api.Tools;

public static class Security {

	public static string HashPassword(string password) {
		return BCrypt.Net.BCrypt.EnhancedHashPassword(password, 14);
	}

	public static bool CheckPassword(string password, string hash) {
		return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
	}
}
