using System.Linq;
using ApiPetFoundation.Application.DTOs.Auth;
using FluentValidation;

namespace ApiPetFoundation.Application.Validators.Auth;

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress();

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
