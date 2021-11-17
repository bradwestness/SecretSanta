using SecretSanta.Models;

namespace SecretSanta.Services;

public interface IAccountRepository
{
    Task<IEnumerable<Account>> GetAllAsync(CancellationToken token);

    Task<Account> GetAsync(Guid id, CancellationToken token);

    Task SaveAsync(Account account, CancellationToken token);

    Task DeleteAsync(Guid id, CancellationToken token);

    void DeleteOldData();
}
