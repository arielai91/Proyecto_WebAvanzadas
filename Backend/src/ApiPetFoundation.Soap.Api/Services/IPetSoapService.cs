using ApiPetFoundation.Application.DTOs.Pets;
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
}
