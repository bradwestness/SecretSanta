using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using Newtonsoft.Json;
using SecretSanta.Models;

namespace SecretSanta.Utilities
{
    public static class DataRepository
    {
        #region Public Methods

        public static void Save<T>(T theObject) where T : class
        {
            var account = theObject as Account;
            if (account != null)
            {
                if (!account.Id.HasValue)
                {
                    account.Id = Guid.NewGuid();
                }

                string output = JsonConvert.SerializeObject(theObject);
                string fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(account.Id.ToString()));
                string directory = GetDataDirectory();
                string path = Path.Combine(directory, fileName);
                File.WriteAllText(path, output, Encoding.UTF8);
            }
        }

        public static T Load<T>(Guid? id) where T : class
        {
            T theObject = null;
            string fileName = string.Empty;

            if (typeof (T) == typeof (Account))
            {
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

        public static void Delete<T>(Guid? id) where T : class
        {
            string fileName = string.Empty;

            if (typeof (T) == typeof (Account))
            {
                fileName = AppSettings.AccountFilePattern.Replace("*", SanitizeFileName(id.ToString()));
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

        public static IList<T> GetAll<T>()
        {
            var list = new List<T>();
            string pattern = string.Empty;

            if (typeof (T) == typeof (Account))
            {
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

            return list;
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