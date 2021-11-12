namespace SecretSanta.Utilities;

public static class GuidEncoder
{
    public static string Encode(Guid input)
    {
        var base64 = Convert.ToBase64String(input.ToByteArray());
        base64 = base64.Replace("/", "_").Replace("+", "-");

        return base64.Substring(0, 22);
    }

    public static Guid? Decode(string encoded)
    {
        Guid? guid = null;

        if (!string.IsNullOrWhiteSpace(encoded))
        {
            encoded = encoded.Replace("_", "/").Replace("-", "+");

            var buffer = Convert.FromBase64String($"{encoded}==");
            guid = new Guid(buffer);
        }

        return guid;
    }
}
