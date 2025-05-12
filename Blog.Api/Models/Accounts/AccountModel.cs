using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace Blog.Api.Models.Accounts;

[Index(nameof(Email))]
public class AccountModel
{
    public enum Roles
    {
        None,
        Admin,
        Owner,
    }

    [Key]
    public required Guid Id { get; init; }
    public Roles Role { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
}
