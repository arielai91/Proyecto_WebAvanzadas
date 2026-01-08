using System.Linq;
using FluentValidation;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Domain.Constants;

namespace ApiPetFoundation.Application.Validators.Pets
{
    public class CreatePetRequestValidator : AbstractValidator<CreatePetRequest>
    {
        public CreatePetRequestValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(50)
                .Must(BeTrimmed)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets);

            RuleFor(x => x.Species)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(BeTrimmed)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets);

            RuleFor(x => x.Breed)
                .NotEmpty()
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(BeTrimmed)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(50);

            RuleFor(x => x.Sex)
                .NotEmpty()
                .Must(PetSexes.IsValid)
                .WithMessage($"Sex must be one of: {string.Join(", ", PetSexes.Allowed)}.");

            RuleFor(x => x.Size)
                .NotEmpty()
                .Must(PetSizes.IsValid)
                .WithMessage($"Size must be one of: {string.Join(", ", PetSizes.Allowed)}.");

            RuleFor(x => x.Description)
                .NotEmpty()
                .MinimumLength(10)
                .MaximumLength(500)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets);
        }

        private static bool BeTrimmed(string? value)
        {
            return value == null || value.Trim().Length == value.Length;
        }

        private static bool NotContainControlChars(string? value)
        {
            return value == null || value.All(ch => !char.IsControl(ch));
        }

        private static bool NotContainAngleBrackets(string? value)
        {
            return value == null || (!value.Contains('<') && !value.Contains('>'));
        }
    }
}
