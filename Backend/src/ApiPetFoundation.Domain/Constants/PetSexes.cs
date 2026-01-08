using System;
using System.Linq;

namespace ApiPetFoundation.Domain.Constants;

public static class PetSexes
{
    public static readonly string[] Allowed = { "Macho", "Hembra" };

    public static bool IsValid(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return false;

        return Allowed.Any(sex => string.Equals(sex, value, StringComparison.OrdinalIgnoreCase));
    }
}
