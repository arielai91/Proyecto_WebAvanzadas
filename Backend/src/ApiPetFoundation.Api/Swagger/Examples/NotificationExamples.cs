using ApiPetFoundation.Application.DTOs.Notifications;
using Swashbuckle.AspNetCore.Filters;

namespace ApiPetFoundation.Api.Swagger.Examples;

public class NotificationResponseExample : IExamplesProvider<NotificationResponse>
{
    public NotificationResponse GetExamples()
    {
        return new NotificationResponse
        {
            Id = 5,
            Type = "AdoptionStatus",
            Message = "Tu solicitud fue aprobada.",
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}
