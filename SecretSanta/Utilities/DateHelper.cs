namespace SecretSanta.Utilities;

public static class DateHelper
{
    public static int Year => (DateTime.Now.Month <= 6)
        ? DateTime.Now.Year - 1
        : DateTime.Now.Year;

    public static bool EnableReceivedGifts => (DateTime.Now.Month <= 6)
        ? true
        : (DateTime.Now.Month == 12 && DateTime.Now.Day > 25);
}
