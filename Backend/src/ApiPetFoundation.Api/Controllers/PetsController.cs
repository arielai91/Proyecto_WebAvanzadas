using System.Linq;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Constants;
using Microsoft.AspNetCore.Mvc;

namespace ApiPetFoundation.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly IEventPublisher _eventPublisher;

        public PetsController(IPetService petService, IEventPublisher eventPublisher)
        {
            _petService = petService;
            _eventPublisher = eventPublisher;
        }

        // GET: api/pets
        [HttpGet]
        public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] string? species = null,
        [FromQuery] string? size = null,
        [FromQuery] string? sex = null,
        [FromQuery] int? createdById = null,
        [FromQuery] int? minAge = null,
        [FromQuery] int? maxAge = null,
        [FromQuery] string? search = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null)
    {
        if (page < 1)
            return BadRequest(new { error = "Page must be greater than 0." });

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new { error = "PageSize must be between 1 and 100." });

        if (!string.IsNullOrWhiteSpace(status)
            && status != PetStatuses.Available
            && status != PetStatuses.Pending
            && status != PetStatuses.Adopted)
            return BadRequest(new { error = "Invalid status filter." });

        if (!string.IsNullOrWhiteSpace(sex) && !PetSexes.IsValid(sex))
            return BadRequest(new { error = "Invalid sex filter." });

        if (!string.IsNullOrWhiteSpace(size) && !PetSizes.IsValid(size))
            return BadRequest(new { error = "Invalid size filter." });

        if (!string.IsNullOrWhiteSpace(species) && species.Length > 30)
            return BadRequest(new { error = "Species filter is too long." });

        if (!string.IsNullOrWhiteSpace(species) && ContainsControlChars(species))
            return BadRequest(new { error = "Species filter contains invalid characters." });

        if (!string.IsNullOrWhiteSpace(search) && search.Length > 100)
            return BadRequest(new { error = "Search filter is too long." });

        if (!string.IsNullOrWhiteSpace(search) && ContainsControlChars(search))
            return BadRequest(new { error = "Search filter contains invalid characters." });

        if (minAge.HasValue && minAge.Value < 0)
            return BadRequest(new { error = "minAge must be 0 or greater." });

        if (maxAge.HasValue && maxAge.Value < 0)
            return BadRequest(new { error = "maxAge must be 0 or greater." });

        if (minAge.HasValue && maxAge.HasValue && minAge.Value > maxAge.Value)
            return BadRequest(new { error = "minAge cannot be greater than maxAge." });

        if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value > createdTo.Value)
            return BadRequest(new { error = "createdFrom cannot be greater than createdTo." });

            var pets = await _petService.GetPagedAsync(
                page,
                pageSize,
                status,
                species,
                size,
                sex,
                createdById,
                minAge,
                maxAge,
                search,
                createdFrom,
                createdTo);

            var response = new
            {
                pets.TotalCount,
                pets.Page,
                pets.PageSize,
                Items = pets.Items.Select(_petService.MapToDetailsResponse)
            };

            return Ok(response);
        }

        // GET: api/pets/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = "Id must be greater than 0." });

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

            await _eventPublisher.PublishAsync("PetCreated", new { Pet = response });

            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, response);
        }

        // PUT: api/pets/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdatePetRequest request)
        {
            if (id <= 0)
                return BadRequest(new { error = "Id must be greater than 0." });

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
            if (id <= 0)
                return BadRequest(new { error = "Id must be greater than 0." });

            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
                return NotFound();

            await _petService.DeletePetAsync(pet);

            return NoContent();
        }

        private static bool ContainsControlChars(string value)
        {
            return value.Any(ch => char.IsControl(ch));
        }
    }
}
