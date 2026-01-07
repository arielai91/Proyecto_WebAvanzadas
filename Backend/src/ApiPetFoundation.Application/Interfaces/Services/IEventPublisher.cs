namespace ApiPetFoundation.Application.Interfaces.Services;

public interface IEventPublisher
{
    Task PublishAsync(string eventName, object? payload);
}
