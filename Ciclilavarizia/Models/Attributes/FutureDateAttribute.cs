using System.ComponentModel.DataAnnotations;

namespace Ciclilavarizia.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
    public class FutureDateAttribute : ValidationAttribute
    {
        public int MinimumDaysInFuture { get; }

        public FutureDateAttribute(int minimumDaysInFuture = 0)
        {
            MinimumDaysInFuture = minimumDaysInFuture;
            ErrorMessage = "The date must be in the future.";
        }

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (value == null) return ValidationResult.Success;

            if (value is DateTime dateTimeValue)
            {
                var minDate = DateTime.UtcNow.AddDays(MinimumDaysInFuture);

                if (dateTimeValue <= minDate)
                {
                    return new ValidationResult(
                        ErrorMessage ?? $"Date must be at least {MinimumDaysInFuture} days in the future.",
                        new[] { validationContext.MemberName! }
                    );
                }

                return ValidationResult.Success;
            }

            return new ValidationResult("Invalid date format.");
        }
    }
}
