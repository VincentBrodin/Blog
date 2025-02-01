using BlogVb.Api.Models.Accounts;
using Microsoft.EntityFrameworkCore;

namespace BlogVb.Api.Services;

public interface IAccountService {
	Task Add(Account account);
	Task<Account?> GetById(Guid id);
	Task<Account?> GetByEmail(string email);
	Task Update(Account account);
	Task Delete(Guid id);
}

public class AccountService : IAccountService {

	private readonly AppDbContext _context;
	public AccountService(AppDbContext context) {
		_context = context;
	}

	public async Task Add(Account account) {
		await _context.AddAsync(account);
		await _context.SaveChangesAsync();
	}

	public Task<Account?> GetById(Guid id) {
		throw new NotImplementedException();
	}
	public async Task<Account?> GetByEmail(string email) {
		return await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);
	}

	public Task Delete(Guid id) {
		throw new NotImplementedException();
	}


	public Task Update(Account account) {
		throw new NotImplementedException();
	}
}
