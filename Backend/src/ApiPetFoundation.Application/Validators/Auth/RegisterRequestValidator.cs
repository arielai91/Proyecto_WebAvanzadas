using System.Linq;
using ApiPetFoundation.Application.DTOs.Auth;
using FluentValidation;

namespace ApiPetFoundation.Application.Validators.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

        RuleFor(x => x.Name)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(6)
            .MaximumLength(100)
            .Must(NotContainControlChars);
    }

    private static bool NotContainControlChars(string? value)
    {
        return value == null || value.All(ch => !char.IsControl(ch));
    }
}
