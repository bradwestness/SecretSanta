using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SecretSanta.Validation
{
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

        public override bool IsValid(object value)
        {
            var file = value as IFormFile;
            bool result = true;

            if (file != null)
            {
                result &= (file.Length < MaxBytes);
            }

            return result;
        }

        public void AddValidation(ClientModelValidationContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            MergeAttribute(context.Attributes, "data-val", "true");
            MergeAttribute(context.Attributes, "data-val-maxfilesize", ErrorMessage);
            MergeAttribute(context.Attributes, "data-val-maxfilesize-maxbytes", MaxBytes.ToString());
        }

        private void MergeAttribute(IDictionary<string, string> attributes, string key, string value)
        {
            if (attributes.ContainsKey(key))
            {
                attributes[key] = value;
            }
            else
            {
                attributes.Add(key, value);
            }
        }
    }
}
