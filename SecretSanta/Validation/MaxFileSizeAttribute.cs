using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace SecretSanta.Validation;

[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
public class MaxFileSizeAttribute : ValidationAttribute, IClientModelValidator
{
    public int MaxBytes { get; set; }

    public MaxFileSizeAttribute(int maxBytes)
        : base("File exceeds the maximum supported size.")
    {
        MaxBytes = maxBytes;
        ErrorMessage = $"Please upload a file of less than {maxBytes} bytes.";
    }

    public override bool IsValid(object? value) =>
        value is null
        || value is IFormFile file && file.Length < MaxBytes;

    public void AddValidation(ClientModelValidationContext context)
    {
        if (context == null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        context.Attributes["data-val"] = "true";
        context.Attributes["data-val-maxfilesize"] = ErrorMessage ?? string.Empty;
        context.Attributes["data-val-maxfilesize-maxbytes"] = MaxBytes.ToString();
    }
}
