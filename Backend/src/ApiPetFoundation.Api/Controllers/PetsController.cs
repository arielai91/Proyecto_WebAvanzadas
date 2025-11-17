using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Services;
using ApiPetFoundation.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace ApiPetFoundation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly PetService _petService;

        public PetsController(PetService petService)
        {
            _petService = petService;
        }

        // GET: api/pets
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var pets = await _petService.GetAllPetsAsync();
            var response = pets.Select(_petService.MapToResponse);
            return Ok(response);
        }

        // GET: api/pets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
                return NotFound();

            return Ok(_petService.MapToResponse(pet));
        }

        // POST: api/pets
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreatePetRequest request)
        {
            int userId = 1; // Temporal, hasta implementar JWT

            var pet = _petService.CreatePetFromDto(request, userId);
            await _petService.AddPetAsync(pet);

            var response = _petService.MapToResponse(pet);

            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, response);
        }

        // PUT: api/pets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePetRequest request)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
                return NotFound();

            _petService.UpdatePetFromDto(pet, request);
            await _petService.UpdatePetAsync(pet);

            return NoContent();
        }

        // DELETE: api/pets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
                return NotFound();

            await _petService.DeletePetAsync(pet);

            return NoContent();
        }
    }
}
