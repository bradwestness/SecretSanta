using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SecretSanta.Models;

namespace SecretSanta.Utilities;

public static class AccountRepository
{
    private static string _contentRootPath = string.Empty;

    private static IList<Account> _accounts_singleton = new List<Account>();

    private static readonly object _lock = new();

    private static IList<Account> Accounts
    {
        get
        {
            lock (_lock)
            {
                return _accounts_singleton;
            }
        }

        set
        {
            lock (_lock)
            {
                _accounts_singleton = value;
            }
        }
    }

    public static void Initialize(string contentRootPath)
    {
        _contentRootPath = contentRootPath;
        DeleteOldData();
    }

    public static Account Get(Guid id)
    {
        if (Accounts.Any())
        {
            return Accounts.Single(a => a.Id == id);
        }

        var fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(id.ToString()));
        Account? account = null;

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var directory = GetDataDirectory();
            var path = Path.Combine(directory, fileName);

            if (File.Exists(path))
            {
                string input = File.ReadAllText(path);
                account = JsonSerializer.Deserialize<Account>(input);
            }
        }

        return account ?? throw new FileNotFoundException(nameof(id));
    }

    public static IList<Account> GetAll()
    {
        var list = new List<Account>();

        foreach (string file in Directory.EnumerateFiles(GetDataDirectory(), AppSettings.AccountFilePattern))
        {
            var input = File.ReadAllText(file);
            var item = JsonSerializer.Deserialize<Account>(input);

            if (item is not null)
            {
                list.Add(item);
            }
        }

        Accounts = list;

        return list;
    }

    public static void Save(Account account)
    {
        if (!account.Id.HasValue)
        {
            account.Id = Guid.NewGuid();
        }

        var output = JsonSerializer.Serialize(account);
        var fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(account.Id.ToString()));

        var directory = GetDataDirectory();
        var path = Path.Combine(directory, fileName);

        File.WriteAllText(path, output, Encoding.UTF8);
        Accounts.Clear();
    }

    public static void Delete(Guid? id)
    {
        var fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(id.ToString()));

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            var directory = GetDataDirectory();
            var path = Path.Combine(directory, fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
                Accounts.Clear();
            }
        }
    }

    private static string SanitizeFileName(string? fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return Regex.Replace(fileName.ToLowerInvariant(), @"[^a-z0-9-]", "");
    }

    private static string GetDataDirectory()
    {
        string directory = Path.Combine(_contentRootPath, AppSettings.DataDirectory);

        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        return directory;
    }

    public static void DeleteOldData()
    {
        var mostRecentYearToKeep = DateHelper.Year - 1;
        var accounts = GetAll();

        foreach (var account in accounts)
        {
            for (var i = DateTime.MinValue.Year; i < mostRecentYearToKeep; i++)
            {
                if (account.Picked?.ContainsKey(i) ?? false)
                {
                    account.Picked.Remove(i);
                }

                if (account.ReceivedGift?.ContainsKey(i) ?? false)
                {
                    account.ReceivedGift.Remove(i);
                }
            }

            Save(account);
        }
    }
}
