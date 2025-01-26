using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CosmosCli.Validations;

class ValidateKeyAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var base64Pattern = @"^(?:[A-Za-z0-9+/]{4})*(?:[A-Za-z0-9+/]{2}==|[A-Za-z0-9+/]{3}=)?$";
        var regex = new Regex(base64Pattern);

        return value is string base64 && base64.Length > 0 && regex.IsMatch(base64)
            ? ValidationResult.Success
            : new ValidationResult($"Key is not valid: '{value}'.");
    }
}