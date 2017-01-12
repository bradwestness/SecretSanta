using System;

namespace SecretSanta.Utilities
{
    public static class DateHelper
    {
        public static int Year => (DateTime.Now.Month <= 6)
            ? DateTime.Now.Year - 1
            : DateTime.Now.Year;
    }
}