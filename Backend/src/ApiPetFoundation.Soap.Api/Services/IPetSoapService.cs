using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Soap.Api.Models;
using System.Collections.Generic;
using System.ServiceModel;

namespace ApiPetFoundation.Soap.Api.Services;

[ServiceContract]
public interface IPetSoapService
{
    [OperationContract]
    Task<IReadOnlyList<PetResponse>> GetPetsAsync();

    [OperationContract]
    Task<PetResponse?> GetPetByIdAsync(int id);

    [OperationContract]
    Task<IReadOnlyList<PetResponse>> GetPetsByStatusAsync(string status);

    [OperationContract]
    Task<IReadOnlyList<AdoptionRequestResponse>> GetAdoptionRequestsByStatusAsync(
        string status,
        DateTime? from,
        DateTime? to);

    [OperationContract]
    Task<AdoptionSummaryResponse> GetAdoptionSummaryAsync(DateTime? from, DateTime? to);
}
