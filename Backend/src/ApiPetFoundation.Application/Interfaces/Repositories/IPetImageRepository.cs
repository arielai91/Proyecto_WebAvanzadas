using ApiPetFoundation.Domain.Entities;

namespace ApiPetFoundation.Application.Interfaces.Repositories
{
    public interface IPetImageRepository : IRepository<PetImage>
    {
        Task<List<PetImage>> GetByPetIdAsync(int petId);
    }
}
