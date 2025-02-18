using Blog.Api.Models.Accounts;
using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext {
	public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {
		Database.Migrate();
	}
	public DbSet<AccountModel> Accounts { get; set; }
}

