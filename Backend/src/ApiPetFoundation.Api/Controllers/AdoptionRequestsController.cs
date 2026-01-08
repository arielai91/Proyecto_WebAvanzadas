using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using ApiPetFoundation.Application.Exceptions;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using Swashbuckle.AspNetCore.Filters;
using ApiPetFoundation.Api.Swagger.Examples;

namespace ApiPetFoundation.Api.Controllers;

/// <summary>Gestion de solicitudes de adopcion.</summary>
[ApiController]
[Route("api/[controller]")]
public class AdoptionRequestsController : ControllerBase
{
    private readonly IAdoptionRequestService _adoptionRequestService;
    private readonly IUserProfileService _userProfileService;
    private readonly IEventPublisher _eventPublisher;

    public AdoptionRequestsController(
        IAdoptionRequestService adoptionRequestService,
        IUserProfileService userProfileService,
        IEventPublisher eventPublisher)
    {
        _adoptionRequestService = adoptionRequestService;
        _userProfileService = userProfileService;
        _eventPublisher = eventPublisher;
    }

    [HttpGet]
    [Authorize]
    /// <summary>Lista solicitudes con filtros y paginado (User, Admin).</summary>
    /// <param name="page">Numero de pagina.</param>
    /// <param name="pageSize">Tamano de pagina (1-100).</param>
    /// <param name="status">Estado de la solicitud.</param>
    /// <param name="petId">Id de la mascota.</param>
    /// <param name="userId">Id del usuario solicitante.</param>
    /// <param name="decisionById">Id del usuario que decidio.</param>
    /// <param name="createdFrom">Fecha inicio.</param>
    /// <param name="createdTo">Fecha fin.</param>
    /// <returns>Listado paginado de solicitudes.</returns>
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? status = null,
        [FromQuery] int? petId = null,
        [FromQuery] int? userId = null,
        [FromQuery] int? decisionById = null,
        [FromQuery] DateTime? createdFrom = null,
        [FromQuery] DateTime? createdTo = null)
    {
        if (page < 1)
            return BadRequest(new { error = "Page must be greater than 0." });

        if (pageSize < 1 || pageSize > 100)
            return BadRequest(new { error = "PageSize must be between 1 and 100." });

        if (!string.IsNullOrWhiteSpace(status)
            && status != AdoptionRequestStatuses.Pending
            && status != AdoptionRequestStatuses.Approved
            && status != AdoptionRequestStatuses.Rejected
            && status != AdoptionRequestStatuses.Cancelled)
            return BadRequest(new { error = "Invalid status filter." });

        if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value > createdTo.Value)
            return BadRequest(new { error = "createdFrom cannot be greater than createdTo." });

        var isAdmin = User.IsInRole("Admin");
        int? effectiveUserId = userId;
        int? effectiveDecisionById = decisionById;

        if (!isAdmin)
        {
            var currentUserId = await GetDomainUserIdAsync();
            if (currentUserId == null)
                return Unauthorized();

            effectiveUserId = currentUserId;
            effectiveDecisionById = null;
        }

        var requests = await _adoptionRequestService.GetPagedWithDetailsAsync(
            page,
            pageSize,
            status,
            petId,
            effectiveUserId,
            effectiveDecisionById,
            createdFrom,
            createdTo);

        var response = new
        {
            requests.TotalCount,
            requests.Page,
            requests.PageSize,
            Items = requests.Items.Select(_adoptionRequestService.MapToDetailsResponse)
        };

        return Ok(response);
    }

    [HttpGet("{id}")]
    [Authorize]
    /// <summary>Obtiene una solicitud por id (User, Admin).</summary>
    /// <param name="id">Id de la solicitud.</param>
    /// <returns>Solicitud encontrada.</returns>
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "Id must be greater than 0." });

        var request = await _adoptionRequestService.GetAdoptionRequestByIdAsync(id);
        if (request == null)
            return NotFound();

        if (!User.IsInRole("Admin"))
        {
            var currentUserId = await GetDomainUserIdAsync();
            if (currentUserId == null)
                return Unauthorized();

            if (request.UserId != currentUserId.Value)
                return Forbid();
        }

        return Ok(_adoptionRequestService.MapToResponse(request));
    }

    [HttpPost]
    [Authorize]
    /// <summary>Crea una solicitud de adopcion (User, Admin).</summary>
    /// <param name="requestDto">Datos de la solicitud.</param>
    /// <returns>Solicitud creada.</returns>
    [SwaggerRequestExample(typeof(CreateAdoptionRequestRequest), typeof(CreateAdoptionRequestExample))]
    [SwaggerResponseExample(201, typeof(AdoptionRequestResponseExample))]
    public async Task<IActionResult> Create([FromBody] CreateAdoptionRequestRequest requestDto)
    {
        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var adoptionRequest = await _adoptionRequestService.CreateRequestAsync(requestDto, userId.Value);
            var response = _adoptionRequestService.MapToResponse(adoptionRequest);

            await _eventPublisher.PublishAsync("AdoptionRequestCreated", new { AdoptionRequest = response });

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/approve")]
    [Authorize(Roles = "Admin")]
    /// <summary>Aprueba una solicitud (Admin).</summary>
    /// <param name="id">Id de la solicitud.</param>
    public async Task<IActionResult> Approve(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "Id must be greater than 0." });

        try
        {
            var adminUserId = await GetDomainUserIdAsync();
            if (adminUserId == null)
                return Unauthorized();

            var adoptionRequest = await _adoptionRequestService.ApproveAsync(id, adminUserId.Value);
            var response = _adoptionRequestService.MapToResponse(adoptionRequest);

            await _eventPublisher.PublishAsync(
                "AdoptionStatusChanged",
                new { AdoptionRequest = response, TargetUserId = response.UserId });

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/reject")]
    [Authorize(Roles = "Admin")]
    /// <summary>Rechaza una solicitud (Admin).</summary>
    /// <param name="id">Id de la solicitud.</param>
    public async Task<IActionResult> Reject(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "Id must be greater than 0." });

        try
        {
            var adminUserId = await GetDomainUserIdAsync();
            if (adminUserId == null)
                return Unauthorized();

            var adoptionRequest = await _adoptionRequestService.RejectAsync(id, adminUserId.Value);
            var response = _adoptionRequestService.MapToResponse(adoptionRequest);

            await _eventPublisher.PublishAsync(
                "AdoptionStatusChanged",
                new { AdoptionRequest = response, TargetUserId = response.UserId });

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpPost("{id}/cancel")]
    [Authorize]
    /// <summary>Cancela una solicitud (User, Admin).</summary>
    /// <param name="id">Id de la solicitud.</param>
    public async Task<IActionResult> Cancel(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "Id must be greater than 0." });

        try
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var adoptionRequest = await _adoptionRequestService.CancelAsync(id, userId.Value);
            var response = _adoptionRequestService.MapToResponse(adoptionRequest);

            await _eventPublisher.PublishAsync(
                "AdoptionStatusChanged",
                new { AdoptionRequest = response, TargetUserId = response.UserId });

            return Ok(response);
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
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
}
