using System.ComponentModel.DataAnnotations;

namespace CosmosCli.Validations;

class ValidateAutoscaleThroughputAttribute : ValidationAttribute
{
    protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
    {
        if (value is int and < 1000)
        {
            return new ValidationResult($"Autoscale throughput values must be between 1000 and 1000000 inclusive in increments of 1000. {value} is less than required minimum throughput of 1000.");
        }
        if (value is int and > 1000000)
        {
            return new ValidationResult($"Autoscale throughput values must be between 1000 and 1000000 inclusive in increments of 1000. {value} is greater than required maximum throughput of 1000000.");
        }
        if (value is int && ((int)value % 1000) != 0)
        {
            return new ValidationResult($"Autoscale throughput values must be between 1000 and 1000000 inclusive in increments of 1000. {value} is not an increment of 1000.");
        }
        return ValidationResult.Success;
    }
}