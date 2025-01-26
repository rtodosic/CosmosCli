using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace CosmosCli.Validations;

class ValidateEndpointAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        var pattern = @"^(https?):\/\/[-a-zA-Z0-9+&@#\/%?=~_|!:,.;]*[-a-zA-Z0-9+&@#\/%=~_|]$";
        var regex = new Regex(pattern, RegexOptions.IgnoreCase);

        return value is string uri && uri.Length > 0 && regex.IsMatch(uri)
            ? ValidationResult.Success
            : new ValidationResult($"Endpoint is not a valid URI: '{value}'.");
    }
}