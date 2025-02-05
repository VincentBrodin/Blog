using BlogVb.Api.Tools;
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

	public Account GetAccount() {
		return new Account() {
			Id = Guid.NewGuid(),
			Username = Username,
			Email = Email,
			Password = Password
		};
	}

	public object? Validate(bool accountExists) {
		string? usernameValid = null;
		string? emailValid = null;
		string? passwordValid = null;
		string? repeatPasswordValid = null;
		if(string.IsNullOrEmpty(Username) || Username.Length < 4) {
			usernameValid = "Username has to be atleast 4 characters";
		}

		if(string.IsNullOrEmpty(Email) || !Helper.IsValidEmail(Email)) {
			emailValid = "Not a valid email";
		}
		else if(accountExists) {
			emailValid = "Account with email already exists";
		}

		if(string.IsNullOrEmpty(Password) || Password.Length < 4) {
			passwordValid = "Password has to be atleast 4 characters";
		}

		if(Password != RepeatPassword) {
			repeatPasswordValid = "Passwords do not match";
		}


		if(usernameValid == null && emailValid == null && passwordValid == null && repeatPasswordValid == null) {
			return null;
		}
		else {
			return new {
				Username,
				Email,
				Password,
				RepeatPassword,

				usernameValid,
				emailValid,
				passwordValid,
				repeatPasswordValid
			};
		}
	}
}
