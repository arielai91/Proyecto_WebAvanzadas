using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Constants;
using ApiPetFoundation.Soap.Api.Models;
using System.Collections.Generic;
using System.Linq;

namespace ApiPetFoundation.Soap.Api.Services;

public class PetSoapService : IPetSoapService
{
    private readonly IPetService _petService;
    private readonly IAdoptionRequestService _adoptionRequestService;

    public PetSoapService(
        IPetService petService,
        IAdoptionRequestService adoptionRequestService)
    {
        _petService = petService;
        _adoptionRequestService = adoptionRequestService;
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

    public async Task<IReadOnlyList<PetResponse>> GetPetsByStatusAsync(string status)
    {
        if (!IsValidPetStatus(status))
            throw new ArgumentException("Invalid pet status.", nameof(status));

        var pets = await _petService.GetAllPetsAsync();
        return pets
            .Where(p => p.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .Select(_petService.MapToResponse)
            .ToList();
    }

    public async Task<IReadOnlyList<AdoptionRequestResponse>> GetAdoptionRequestsByStatusAsync(
        string status,
        DateTime? from,
        DateTime? to)
    {
        if (!IsValidAdoptionStatus(status))
            throw new ArgumentException("Invalid adoption status.", nameof(status));

        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentException("From cannot be greater than To.");

        var requests = await _adoptionRequestService.GetAllAdoptionRequestsAsync();
        return requests
            .Where(r => r.Status.Equals(status, StringComparison.OrdinalIgnoreCase))
            .Where(r => !from.HasValue || r.CreatedAt >= from.Value)
            .Where(r => !to.HasValue || r.CreatedAt <= to.Value)
            .Select(_adoptionRequestService.MapToResponse)
            .ToList();
    }

    public async Task<AdoptionSummaryResponse> GetAdoptionSummaryAsync(DateTime? from, DateTime? to)
    {
        if (from.HasValue && to.HasValue && from.Value > to.Value)
            throw new ArgumentException("From cannot be greater than To.");

        var requests = await _adoptionRequestService.GetAllAdoptionRequestsAsync();
        var filtered = requests
            .Where(r => !from.HasValue || r.CreatedAt >= from.Value)
            .Where(r => !to.HasValue || r.CreatedAt <= to.Value)
            .ToList();

        return new AdoptionSummaryResponse
        {
            Total = filtered.Count,
            Pending = filtered.Count(r => r.Status == AdoptionRequestStatuses.Pending),
            Approved = filtered.Count(r => r.Status == AdoptionRequestStatuses.Approved),
            Rejected = filtered.Count(r => r.Status == AdoptionRequestStatuses.Rejected),
            Cancelled = filtered.Count(r => r.Status == AdoptionRequestStatuses.Cancelled),
            From = from,
            To = to
        };
    }

    private static bool IsValidPetStatus(string status)
    {
        return status.Equals(PetStatuses.Available, StringComparison.OrdinalIgnoreCase)
            || status.Equals(PetStatuses.Pending, StringComparison.OrdinalIgnoreCase)
            || status.Equals(PetStatuses.Adopted, StringComparison.OrdinalIgnoreCase);
    }

    private static bool IsValidAdoptionStatus(string status)
    {
        return status.Equals(AdoptionRequestStatuses.Pending, StringComparison.OrdinalIgnoreCase)
            || status.Equals(AdoptionRequestStatuses.Approved, StringComparison.OrdinalIgnoreCase)
            || status.Equals(AdoptionRequestStatuses.Rejected, StringComparison.OrdinalIgnoreCase)
            || status.Equals(AdoptionRequestStatuses.Cancelled, StringComparison.OrdinalIgnoreCase);
    }
}
