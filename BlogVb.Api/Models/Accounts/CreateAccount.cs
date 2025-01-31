using System.Text.Json.Serialization;

namespace BlogVb.Api.Models.Accounts;

public class CreateAccount {
	[JsonPropertyName("username")]
	public string Username { get; set; } = string.Empty;

	[JsonPropertyName("email")]
	public string Email { get; set; } = string.Empty;

	[JsonPropertyName("password")]
	public string Password { get; set; } = string.Empty;

	[JsonPropertyName("repeatPassword")]
	public string RepeatPassword { get; set; } = string.Empty;
}
