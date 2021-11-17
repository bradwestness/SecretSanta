using Microsoft.AspNetCore.Mvc;
using SecretSanta.Models;
using SecretSanta.Services;

namespace SecretSanta.Controllers;

public class PreviewController : Controller
{
    private readonly IAccountRepository _accountRepository;
    private readonly IPreviewGenerator _previewGenerator;

    public PreviewController(IAccountRepository accountRepository, IPreviewGenerator previewGenerator)
    {
        _accountRepository = accountRepository ?? throw new ArgumentNullException(nameof(accountRepository));
        _previewGenerator = previewGenerator ?? throw new ArgumentNullException(nameof(previewGenerator));
    }

    [HttpGet, Route("Preview/{accountId:Guid}/{itemId:Guid}")]
    public async Task<IActionResult> GetPreview(Guid accountId, Guid itemId, CancellationToken token = default)
    {
        var account = await _accountRepository.GetAsync(accountId, token);
        var item = new KeyValuePair<int, WishlistItem>();

        foreach (var year in account.Wishlist.Keys)
        {
            if (account.Wishlist[year].FirstOrDefault(x => x.Id == itemId) is WishlistItem match)
            {
                item = new KeyValuePair<int, WishlistItem>(year, match);
                break;
            }
        }

        if (item.Value.PreviewImage == null || item.Value.PreviewImage.Length == 0)
        {
            account.Wishlist[item.Key].Remove(item.Value);
            item.Value.PreviewImage = await _previewGenerator.GeneratePreviewAsync(item.Value.Url, token);
            account.Wishlist[item.Key].Add(item.Value);

            await _accountRepository.SaveAsync(account, token);
        }

        return File(item.Value.PreviewImage, "image/jpg");
    }
}
