using System.Linq;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Domain.Constants;
using FluentValidation;

namespace ApiPetFoundation.Application.Validators.Pets
{
    public class PatchPetRequestValidator : AbstractValidator<PatchPetRequest>
    {
        public PatchPetRequestValidator()
        {
            RuleFor(x => x.Name)
                .MinimumLength(2)
                .MaximumLength(50)
                .Must(BeTrimmed)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets)
                .When(x => x.Name != null);

            RuleFor(x => x.Species)
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(BeTrimmed)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets)
                .When(x => x.Species != null);

            RuleFor(x => x.Breed)
                .MinimumLength(2)
                .MaximumLength(30)
                .Must(BeTrimmed)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets)
                .When(x => x.Breed != null);

            RuleFor(x => x.Age)
                .GreaterThanOrEqualTo(0)
                .LessThanOrEqualTo(50)
                .When(x => x.Age.HasValue);

            RuleFor(x => x.Sex)
                .Must(PetSexes.IsValid)
                .WithMessage($"Sex must be one of: {string.Join(", ", PetSexes.Allowed)}.")
                .When(x => x.Sex != null);

            RuleFor(x => x.Size)
                .Must(PetSizes.IsValid)
                .WithMessage($"Size must be one of: {string.Join(", ", PetSizes.Allowed)}.")
                .When(x => x.Size != null);

            RuleFor(x => x.Description)
                .MinimumLength(10)
                .MaximumLength(500)
                .Must(NotContainControlChars)
                .Must(NotContainAngleBrackets)
                .When(x => x.Description != null);
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
