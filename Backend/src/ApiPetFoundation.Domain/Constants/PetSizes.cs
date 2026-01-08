using System;
using System.Linq;

namespace ApiPetFoundation.Domain.Constants;

public static class PetSizes
{
    public static readonly string[] Allowed =
    {
        "Pequeno",
        "Pequeño",
        "Mediano",
        "Grande"
    };

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Allowed.Any(size => string.Equals(size, value, StringComparison.OrdinalIgnoreCase));
    }
}
