using System.Text.Json.Serialization;

namespace BlogVb.Api.Models.Accounts;

public class LoginAccount {
	[JsonPropertyName("email")]
	public string Email { get; set; } = string.Empty;

	[JsonPropertyName("password")]
	public string Password { get; set; } = string.Empty;

}
