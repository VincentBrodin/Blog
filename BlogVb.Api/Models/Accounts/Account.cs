namespace BlogVb.Api.Models.Accounts;

public class Account {
	public Guid Id { get; init; }

	public string Username { get; set; } = string.Empty;
	public string Email { get; set; } = string.Empty;
	public string Password { get; set; } = string.Empty;


	public Account() { }
	public Account(CreateAccount createAccount) {
		Id = Guid.NewGuid();
		Username = createAccount.Username;
		Email = createAccount.Email;
		Password = createAccount.Password;
	}
}
