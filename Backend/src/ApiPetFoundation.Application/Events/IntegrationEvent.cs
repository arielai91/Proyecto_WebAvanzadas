using System.Text.Json;

namespace ApiPetFoundation.Application.Events;

public class IntegrationEvent
{
    public string Event { get; set; } = string.Empty;
    public JsonElement? Data { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
    public int Version { get; set; } = 1;
}
