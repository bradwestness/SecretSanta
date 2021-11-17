namespace SecretSanta.Services;

public interface IPreviewGenerator
{
    Task<byte[]> GeneratePreviewAsync(string? url, CancellationToken token);
}
