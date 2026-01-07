using ApiPetFoundation.Application.Exceptions;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ApiPetFoundation.Api.Controllers
{
    [ApiController]
    [Route("api/pets/{petId:int}/image")]
    public class PetImagesController : ControllerBase
    {
        private readonly IStorageService _storageService;
        private readonly PetImageService _petImageService;
        private readonly IPetService _petService;

        public PetImagesController(
            IStorageService storageService,
            PetImageService petImageService,
            IPetService petService)
        {
            _storageService = storageService;
            _petImageService = petImageService;
            _petService = petService;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        [RequestSizeLimit(10_000_000)]
        public async Task<IActionResult> Upload(int petId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "File is required." });

            var pet = await _petService.GetPetByIdAsync(petId);
            if (pet == null)
                return NotFound();

            try
            {
                await using var stream = file.OpenReadStream();
                var url = await _storageService.UploadPetImageAsync(petId, file.FileName, stream, file.ContentType);
                var image = await _petImageService.SetCoverImageAsync(petId, url);

                return Ok(new { imageUrl = image.Url, imageId = image.Id });
            }
            catch (ValidationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
