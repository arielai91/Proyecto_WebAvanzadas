using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using System.Collections.Generic;
using System.Linq;

namespace ApiPetFoundation.Soap.Api.Services;

public class PetSoapService : IPetSoapService
{
    private readonly IPetService _petService;

    public PetSoapService(IPetService petService)
    {
        _petService = petService;
    }

    public async Task<IReadOnlyList<PetResponse>> GetPetsAsync()
    {
        var pets = await _petService.GetAllPetsAsync();
        return pets.Select(_petService.MapToResponse).ToList();
    }

    public async Task<PetResponse?> GetPetByIdAsync(int id)
    {
        var pet = await _petService.GetPetByIdAsync(id);
        return pet is null ? null : _petService.MapToResponse(pet);
    }
}
