using Blog.Api.Models.Accounts;
using Microsoft.EntityFrameworkCore;

namespace Blog.Api.Services;

public interface IAccountService
{
    Task Add(AccountModel account);
    Task<AccountModel?> GetById(Guid id);
    Task<AccountModel?> GetByEmail(string email);
    Task Update(AccountModel account);
    Task Delete(Guid id);
}

public class AccountService : IAccountService
{

    private readonly AppDbContext _context;
    public AccountService(AppDbContext context)
    {
        _context = context;
    }

    public async Task Add(AccountModel account)
    {
        await _context.AddAsync(account);
        await _context.SaveChangesAsync();
    }

    public async Task<AccountModel?> GetById(Guid id)
    {
        return await _context.Accounts.FirstOrDefaultAsync(x => x.Id == id);
    }
    public async Task<AccountModel?> GetByEmail(string email)
    {
        return await _context.Accounts.AsNoTracking().FirstOrDefaultAsync(a => a.Email == email);
    }

    public Task Delete(Guid id)
    {
        throw new NotImplementedException();
    }


    public async Task Update(AccountModel account)
    {
        _context.Update(account);
        await _context.SaveChangesAsync();
    }
}
