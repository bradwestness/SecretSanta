﻿using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using SecretSanta.Models;

namespace SecretSanta.Utilities;

public static class DataRepository
{
    private static string _contentRootPath;
    private static IList<Account> _accounts_singleton;
    private static object _lock = new object();
    private static IList<Account> _accounts
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

    public static T Get<T>(Guid? id) where T : class
    {
        T theObject = null;
        string fileName = string.Empty;

        if (typeof(T) == typeof(Account))
        {
            if (_accounts != null)
            {
                return _accounts.SingleOrDefault(a => a.Id == id) as T;
            }

            fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(id.ToString()));
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            string directory = GetDataDirectory();
            string path = Path.Combine(directory, fileName);

            if (File.Exists(path))
            {
                string input = File.ReadAllText(path);
                theObject = JsonSerializer.Deserialize<T>(input);
            }
        }

        return theObject;
    }

    public static IList<T> GetAll<T>()
    {
        var list = new List<T>();
        string pattern = string.Empty;

        if (typeof(T) == typeof(Account))
        {
            if (_accounts != null)
            {
                return _accounts as IList<T>;
            }

            pattern = AppSettings.AccountFilePattern;
        }

        if (string.IsNullOrWhiteSpace(pattern))
        {
            return list;
        }

        foreach (string file in Directory.EnumerateFiles(GetDataDirectory(), pattern))
        {
            string input = File.ReadAllText(file);
            var item = JsonSerializer.Deserialize<T>(input);
            list.Add(item);
        }

        if (typeof(T) == typeof(Account))
        {
            _accounts = list as IList<Account>;
        }

        return list;
    }

    public static void Save<T>(T theObject) where T : class
    {
        string fileName = string.Empty;
        string output = string.Empty;

        if (typeof(T) == typeof(Account))
        {
            var account = theObject as Account;
            if (account != null)
            {
                if (!account.Id.HasValue)
                {
                    account.Id = Guid.NewGuid();
                }
            }

            output = JsonSerializer.Serialize(account);
            fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(account.Id.ToString()));
            _accounts = null;
        }

        string directory = GetDataDirectory();
        string path = Path.Combine(directory, fileName);
        File.WriteAllText(path, output, Encoding.UTF8);
    }

    public static void Delete<T>(Guid? id) where T : class
    {
        string fileName = string.Empty;

        if (typeof(T) == typeof(Account))
        {
            fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(id.ToString()));
            _accounts = null;
        }

        if (!string.IsNullOrWhiteSpace(fileName))
        {
            string directory = GetDataDirectory();
            string path = Path.Combine(directory, fileName);

            if (File.Exists(path))
            {
                File.Delete(path);
            }
        }
    }

    private static string SanitizeFileName(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            return string.Empty;
        }

        return Regex.Replace(fileName.ToLower(), @"[^a-z0-9-]", "");
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
        var accounts = GetAll<Account>();

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
