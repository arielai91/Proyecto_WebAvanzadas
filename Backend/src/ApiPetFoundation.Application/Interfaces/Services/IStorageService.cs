namespace ApiPetFoundation.Application.Interfaces.Services
{
    public interface IStorageService
    {
        Task<string> UploadPetImageAsync(int petId, string fileName, Stream content, string contentType);
    }
}
