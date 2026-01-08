using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using ApiPetFoundation.Application.DTOs.Pets;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Filters;
using ApiPetFoundation.Api.Swagger.Examples;

namespace ApiPetFoundation.Api.Controllers
{
    /// <summary>Gestion de mascotas.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class PetsController : ControllerBase
    {
        private readonly IPetService _petService;
        private readonly IEventPublisher _eventPublisher;
        private readonly IUserProfileService _userProfileService;

        public PetsController(
            IPetService petService,
            IEventPublisher eventPublisher,
            IUserProfileService userProfileService)
        {
            _petService = petService;
            _eventPublisher = eventPublisher;
            _userProfileService = userProfileService;
        }

        // GET: api/pets
        /// <summary>Lista mascotas con filtros y paginado (Publico).</summary>
        /// <param name="page">Numero de pagina.</param>
        /// <param name="pageSize">Tamano de pagina (1-100).</param>
        /// <param name="status">Estado de la mascota.</param>
        /// <param name="species">Especie de la mascota.</param>
        /// <param name="size">Tamano de la mascota.</param>
        /// <param name="sex">Sexo de la mascota.</param>
        /// <param name="createdById">Id del usuario que creo la mascota.</param>
        /// <param name="minAge">Edad minima.</param>
        /// <param name="maxAge">Edad maxima.</param>
        /// <param name="search">Busqueda por nombre/descripcion.</param>
        /// <param name="createdFrom">Fecha inicio.</param>
        /// <param name="createdTo">Fecha fin.</param>
        /// <returns>Listado paginado de mascotas.</returns>
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
        /// <summary>Obtiene una mascota por id (Publico).</summary>
        /// <param name="id">Id de la mascota.</param>
        /// <returns>Mascota encontrada.</returns>
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
        /// <summary>Crea una mascota (Admin).</summary>
        /// <param name="request">Datos de la mascota.</param>
        /// <returns>Mascota creada.</returns>
        [HttpPost]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(CreatePetRequest), typeof(CreatePetRequestExample))]
        [SwaggerResponseExample(201, typeof(PetResponseExample))]
        public async Task<IActionResult> Create([FromBody] CreatePetRequest request)
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var pet = _petService.CreatePetFromDto(request, userId.Value);
            await _petService.AddPetAsync(pet);

            var response = _petService.MapToResponse(pet);

            await _eventPublisher.PublishAsync("PetCreated", new { Pet = response });

            return CreatedAtAction(nameof(GetById), new { id = pet.Id }, response);
        }

        // PUT: api/pets/{id}
        /// <summary>Actualiza todos los campos de una mascota (Admin).</summary>
        /// <param name="id">Id de la mascota.</param>
        /// <param name="request">Datos completos de la mascota.</param>
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(UpdatePetRequest), typeof(UpdatePetRequestExample))]
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

        // PATCH: api/pets/{id}
        /// <summary>Actualiza campos parciales de una mascota (Admin).</summary>
        /// <param name="id">Id de la mascota.</param>
        /// <param name="request">Campos a actualizar.</param>
        [HttpPatch("{id}")]
        [Authorize(Roles = "Admin")]
        [SwaggerRequestExample(typeof(PatchPetRequest), typeof(PatchPetRequestExample))]
        public async Task<IActionResult> Patch(int id, [FromBody] PatchPetRequest request)
        {
            if (id <= 0)
                return BadRequest(new { error = "Id must be greater than 0." });

            var pet = await _petService.GetPetByIdAsync(id);
            if (pet == null)
                return NotFound();

            _petService.PatchPetFromDto(pet, request);
            await _petService.UpdatePetAsync(pet);

            return NoContent();
        }

        // DELETE: api/pets/{id}
        /// <summary>Elimina una mascota (Admin).</summary>
        /// <param name="id">Id de la mascota.</param>
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
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

        private async Task<int?> GetDomainUserIdAsync()
        {
            var identityUserId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue(JwtRegisteredClaimNames.Sub)
                ?? User.FindFirstValue("sub");

            if (string.IsNullOrWhiteSpace(identityUserId))
                return null;

            var user = await _userProfileService.GetByIdentityUserIdAsync(identityUserId);
            return user?.Id;
        }

        private static bool ContainsControlChars(string value)
        {
            return value.Any(ch => char.IsControl(ch));
        }
    }
}
