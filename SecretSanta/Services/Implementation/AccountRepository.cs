using System.Collections.Concurrent;
using System.Text.Json;
using SecretSanta.Models;
using SecretSanta.Utilities;

namespace SecretSanta.Services.Implementation;

public class AccountRepository : IAccountRepository
{
    private static readonly ConcurrentDictionary<Guid, Account> _accounts = new ConcurrentDictionary<Guid, Account>();

    private readonly IWebHostEnvironment _environment;

    private readonly IAppSettings _appSettings;

    public AccountRepository(IWebHostEnvironment environment, IAppSettings appSettings)
    {
        _appSettings = appSettings ?? throw new ArgumentNullException(nameof(appSettings));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));

        DeleteOldData();
    }

    public async Task<IEnumerable<Account>> GetAllAsync(CancellationToken token)
    {
        string accountFileSuffix = _appSettings.AccountFilePattern.Replace("*", string.Empty);

        string removeAccountFileSuffix(string filePath) =>
            Path.GetFileNameWithoutExtension(filePath)
                .Replace(accountFileSuffix, string.Empty)
                .Trim(new[] { ' ', '.' });

        bool isAccountFile(string filePath) => Guid.TryParseExact(removeAccountFileSuffix(filePath), "N", out Guid _);

        if (!_accounts.Any())
        {
            foreach (var filePath in Directory.EnumerateFiles(DataDirectory, _appSettings.AccountFilePattern))
            {
                if (!isAccountFile(filePath))
                {
                    continue;
                }

                using var file = File.OpenRead(filePath);

                var account = await JsonSerializer.DeserializeAsync<Account>(file, cancellationToken: token);

                if (account is null || !account.Id.HasValue)
                {
                    continue;
                }

                _accounts.AddOrUpdate(account.Id.Value, account, (id, toUpdate) =>
                {
                    toUpdate.DisplayName = account.DisplayName;
                    toUpdate.DoNotPick = account.DoNotPick;
                    toUpdate.Email = account.Email;
                    toUpdate.Id = account.Id;
                    toUpdate.Picked = account.Picked;
                    toUpdate.ReceivedGift = account.ReceivedGift;
                    toUpdate.Wishlist = account.Wishlist;

                    return toUpdate;
                });
            }
        }

        return _accounts.Values;
    }

    public async Task<Account> GetAsync(Guid id, CancellationToken token)
    {
        var accounts = await GetAllAsync(token);
        return accounts.Single(a => a.Id == id);
    }

    public async Task SaveAsync(Account account, CancellationToken token)
    {
        if (account is null)
        {
            throw new ArgumentNullException(nameof(account));
        }

        if (!account.Id.HasValue || account.Id.Value == Guid.Empty)
        {
            account.Id = Guid.NewGuid();
        }

        var filePath = GetAccountFilePath(account.Id.Value);

        using var file = File.Open(filePath, FileMode.Create, FileAccess.Write, FileShare.None);

        await JsonSerializer.SerializeAsync(file, account, cancellationToken: token);

        _accounts.Clear();
    }

    public Task DeleteAsync(Guid id, CancellationToken token) =>
        Task.Run(() =>
        {
            var filePath = GetAccountFilePath(id);

            try
            {
                File.Delete(filePath);
                _accounts.Clear();
            }
            catch
            {

            }
        }, token);

    public void DeleteOldData()
    {
        var mostRecentYearToKeep = DateHelper.Year - 1;
        var accounts = GetAllAsync(default).GetAwaiter().GetResult();
        var count = accounts.Count();
        var tasks = new Task[count];

        for (var i = 0; i < count; i++)
        {
            var account = accounts.ElementAt(i);
            var isUpdated = false;

            for (var year = DateTime.MinValue.Year; year < mostRecentYearToKeep; year++)
            {
                if (account.Picked?.ContainsKey(year) ?? false)
                {
                    account.Picked.Remove(year);
                    isUpdated = true;
                }

                if (account.ReceivedGift?.ContainsKey(year) ?? false)
                {
                    account.ReceivedGift.Remove(year);
                    isUpdated = true;
                }
            }

            tasks[i] = isUpdated
                ? SaveAsync(account, default)
                : Task.CompletedTask;
        }

        Task.WaitAll(tasks);
    }

    private string DataDirectory =>
        Path.Combine(_environment.ContentRootPath, _appSettings.DataDirectory);

    private string GetAccountFileName(Guid id)
    {
        var idString = id.ToString("N").ToLowerInvariant();
        var accountFileName = _appSettings.AccountFilePattern.Replace("*", idString);

        return accountFileName;
    }

    private string GetAccountFilePath(Guid id) =>
        Path.Combine(DataDirectory, GetAccountFileName(id));
}
