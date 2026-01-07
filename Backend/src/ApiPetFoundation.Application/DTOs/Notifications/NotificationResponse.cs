namespace ApiPetFoundation.Application.DTOs.Notifications
{
    public class NotificationResponse
    {
        public int Id { get; init; }
        public string Type { get; init; } = string.Empty;
        public string Message { get; init; } = string.Empty;
        public bool IsRead { get; init; }
        public DateTime CreatedAt { get; init; }
    }
}

