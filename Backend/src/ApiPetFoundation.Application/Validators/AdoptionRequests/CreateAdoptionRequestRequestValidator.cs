using System.Linq;
using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using FluentValidation;

namespace ApiPetFoundation.Application.Validators.AdoptionRequests;

public class CreateAdoptionRequestRequestValidator : AbstractValidator<CreateAdoptionRequestRequest>
{
    public CreateAdoptionRequestRequestValidator()
    {
        RuleFor(x => x.PetId)
            .GreaterThan(0);

        RuleFor(x => x.Message)
            .MaximumLength(500)
            .Must(BeTrimmed)
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
