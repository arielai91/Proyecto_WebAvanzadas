using System.Linq;
using ApiPetFoundation.Application.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace ApiPetFoundation.Api.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly IUserProfileService _userProfileService;

        public NotificationsController(
            INotificationService notificationService,
            IUserProfileService userProfileService)
        {
            _notificationService = notificationService;
            _userProfileService = userProfileService;
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetMyNotifications(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            [FromQuery] string? type = null,
            [FromQuery] bool? isRead = null,
            [FromQuery] DateTime? createdFrom = null,
            [FromQuery] DateTime? createdTo = null)
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            if (page < 1)
                return BadRequest(new { error = "Page must be greater than 0." });

            if (pageSize < 1 || pageSize > 100)
                return BadRequest(new { error = "PageSize must be between 1 and 100." });

            if (!string.IsNullOrWhiteSpace(type) && type.Length > 50)
                return BadRequest(new { error = "Type filter is too long." });

            if (!string.IsNullOrWhiteSpace(type) && ContainsControlChars(type))
                return BadRequest(new { error = "Type filter contains invalid characters." });

            if (createdFrom.HasValue && createdTo.HasValue && createdFrom.Value > createdTo.Value)
                return BadRequest(new { error = "createdFrom cannot be greater than createdTo." });

            var notifications = await _notificationService.GetPagedAsync(
                userId.Value,
                page,
                pageSize,
                type,
                isRead,
                createdFrom,
                createdTo);
            var response = new
            {
                notifications.TotalCount,
                notifications.Page,
                notifications.PageSize,
                Items = notifications.Items.Select(_notificationService.MapToResponse)
            };

            return Ok(response);
        }

        [HttpGet("unread")]
        [Authorize]
        public async Task<IActionResult> GetUnread()
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var notifications = await _notificationService.GetUnreadByUserAsync(userId.Value);
            var response = notifications.Select(_notificationService.MapToResponse);
            return Ok(response);
        }

        [HttpPost("{id}/read")]
        [Authorize]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            if (id <= 0)
                return BadRequest(new { error = "Id must be greater than 0." });

            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var notification = await _notificationService.GetNotificationByIdAsync(id);
            if (notification == null || notification.UserId != userId.Value)
                return NotFound();

            await _notificationService.MarkAsReadAsync(notification);
            return NoContent();
        }

        [HttpPost("read-all")]
        [Authorize]
        public async Task<IActionResult> MarkAllAsRead()
        {
            var userId = await GetDomainUserIdAsync();
            if (userId == null)
                return Unauthorized();

            var updatedCount = await _notificationService.MarkAllAsReadAsync(userId.Value);
            return Ok(new { updated = updatedCount });
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
