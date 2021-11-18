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

    public static IHtmlContent ResultMessage(this IHtmlHelper helper)
    {
        helper.ViewContext.HttpContext.Session.TryGetValue("ResultMessage", out var messageBytes);
        var output = string.Empty;

        if (messageBytes?.Length > 0)
        {
            var message = Encoding.UTF8.GetString(messageBytes);

            if (!string.IsNullOrWhiteSpace(message))
            {
                output = $@"
<div class='alert alert-info alert-dismissable'>
    <button type='button' class='close' data-dismiss='alert' aria-hidden='true'>&times;</button>
    {message}
</div>
";
            }

            helper.ViewContext.HttpContext.Session.Remove("ResultMessage");
        }

        return new HtmlString(output);
    }
}
