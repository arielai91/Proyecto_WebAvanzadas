using ApiPetFoundation.Application.DTOs.AdoptionRequests;
using ApiPetFoundation.Application.Exceptions;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;

namespace ApiPetFoundation.Api.Controllers;

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

        var requests = await _adoptionRequestService.GetPagedWithDetailsAsync(
            page,
            pageSize,
            status,
            petId,
            userId,
            decisionById,
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
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { error = "Id must be greater than 0." });

        var request = await _adoptionRequestService.GetAdoptionRequestByIdAsync(id);
        if (request == null)
            return NotFound();

        return Ok(_adoptionRequestService.MapToResponse(request));
    }

    [HttpPost]
    [Authorize]
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

