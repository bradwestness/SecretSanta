using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using SecretSanta.Models;

namespace SecretSanta.Utilities
{
    public static class DataRepository
    {
        #region Variables

        private static IList<Account> _accounts;

        #endregion

        #region Public Methods

        public static T Get<T>(Guid? id) where T : class
        {
            T theObject = null;
            string fileName = string.Empty;

            if (typeof (T) == typeof (Account))
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
                    theObject = JsonConvert.DeserializeObject<T>(input);
                }
            }

            return theObject;
        }

        public static IList<T> GetAll<T>()
        {
            var list = new List<T>();
            string pattern = string.Empty;

            if (typeof (T) == typeof (Account))
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
                var item = JsonConvert.DeserializeObject<T>(input);
                list.Add(item);
            }

            if (typeof (T) == typeof (Account))
            {
                _accounts = list as IList<Account>;
            }

            return list;
        }

        public static void Save<T>(T theObject) where T : class
        {
            string fileName = string.Empty;
            string output = string.Empty;

            if (typeof (T) == typeof (Account))
            {
                var account = theObject as Account;
                if (account != null)
                {
                    if (!account.Id.HasValue)
                    {
                        account.Id = Guid.NewGuid();
                    }
                }

                output = JsonConvert.SerializeObject(account);
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

            if (typeof (T) == typeof (Account))
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

        #endregion

        #region Private Methods

        private static string SanitizeFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName))
                return string.Empty;

            return Regex.Replace(fileName.ToLower(), @"[^a-z0-9-]", "");
        }

        private static string GetDataDirectory()
        {
            string directory = HttpContext.Current.Server.MapPath(AppSettings.DataDirectory);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            return directory;
        }

        #endregion
    }
}