using Humanizer;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Web;
using System.Web.Mvc;

namespace SecretSanta
{
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
    public class MaxFileSizeAttribute : ValidationAttribute, IClientValidatable
    {
        public int? MaxBytes { get; set; }

        public MaxFileSizeAttribute(int maxBytes)
            : base("File exceeds the maximum supported size.")
        {
            MaxBytes = maxBytes;
            ErrorMessage = $"Please upload a file of less than {maxBytes.Bytes()}.";
        }

        public override bool IsValid(object value)
        {
            HttpPostedFileBase file = value as HttpPostedFileBase;
            bool result = true;

            if (file != null && MaxBytes.HasValue)
            {
                result &= (file.ContentLength < MaxBytes.Value);
            }

            return result;
        }

        public IEnumerable<ModelClientValidationRule> GetClientValidationRules(ModelMetadata metadata, ControllerContext context)
        {
            var rule = new ModelClientValidationRule
            {
                ValidationType = "maxfilesize",
                ErrorMessage = FormatErrorMessage(metadata.DisplayName)
            };
            rule.ValidationParameters["maxbytes"] = MaxBytes;
            yield return rule;
        }
    }
}