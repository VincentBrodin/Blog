using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BlogVb.Api.Models.Accounts;

[Index(nameof(Email))]
public class Account {
	[Key]
	public required Guid Id { get; init; }
	public required string Username { get; set; }
	public required string Email { get; set; }
	public required string Password { get; set; }
}
