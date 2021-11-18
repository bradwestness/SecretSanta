using System.Text;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using SecretSanta.Models;
using SecretSanta.Services;

namespace SecretSanta.Utilities;

public static class ControllerExtensions
{
    public static async Task<Account?> GetAccountAsync(
        this Controller controller,
        IAccountRepository accountRepository,
        CancellationToken token)
    {
        if (controller?.User?.Identity?.IsAuthenticated != true ||
            string.IsNullOrEmpty(controller?.User?.Identity?.Name))
        {
            return null;
        }

        var email = controller.User.Identity.Name;
        var accounts = await accountRepository.GetAllAsync(token);
        var account = accounts.SingleOrDefault(a => a.Email?.Equals(email, StringComparison.OrdinalIgnoreCase) ?? false);

        return account;
    }

    public static void SetResultMessage(this Controller controller, string message) =>
        controller.HttpContext.Session.Set("ResultMessage", Encoding.UTF8.GetBytes(message));
}
