using System.Text;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace SecretSanta.TagHelpers;

[HtmlTargetElement("result-message", TagStructure = TagStructure.NormalOrSelfClosing)]
public class ResultMessageTagHelper : TagHelper
{
    private const string CloseButton = @"<button type=""button"" class=""btn-close"" data-bs-dismiss=""alert"" />";

    [ViewContext]
    public ViewContext? ViewContext { get; set; }

    public override void Process(TagHelperContext context, TagHelperOutput output)
    {
        if (ViewContext?.HttpContext.Session.Get("ResultMessage") is not byte[] bytes
            || Encoding.UTF8.GetString(bytes) is not string message
            || string.IsNullOrWhiteSpace(message))
        {
            output.SuppressOutput();
            return;
        }

        output.TagName = "div";

        output.Attributes.SetAttribute("class", "alert alert-info alert-dismissable");
        output.Attributes.SetAttribute("role", "alert");

        output.Content.SetHtmlContent($@"{message} {CloseButton}");

        ViewContext?.HttpContext.Session.Remove("ResultMessage");
    }
}
