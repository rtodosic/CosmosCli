using System.ComponentModel.DataAnnotations;

namespace CosmosCli.Validations;

class ValidateManualThroughputAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int and < 400)
        {
            return new ValidationResult($"Manual throughput values must be between 400 and 1000000 inclusive in increments of 100. {value} is less than required minimum throughput of 400.");
        }

        if (value is int and > 1000000)
        {
            return new ValidationResult($"Manual throughput values must be between 400 and 1000000 inclusive in increments of 100. {value} is greater than required maximum throughput of 1000000.");
        }

        if (value is int && ((int)value % 100) != 0)
        {
            return new ValidationResult($"Manual throughput values must be between 400 and 1000000 inclusive in increments of 100. {value} is not an increment of 100.");
        }
        return ValidationResult.Success;
    }
}