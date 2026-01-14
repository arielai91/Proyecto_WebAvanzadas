using System.Text;
using System.Text.Json;
using ApiPetFoundation.Application.Events;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Domain.Entities;
using ApiPetFoundation.Infrastructure.Configuration;
using ApiPetFoundation.Notifications.Api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace ApiPetFoundation.Notifications.Api.BackgroundServices;

public class RabbitMqPetEventSubscriber : BackgroundService
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqPetEventSubscriber> _logger;
    private readonly IHubContext<NotificationHub> _hubContext;
    private readonly IServiceProvider _serviceProvider;
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPetEventSubscriber(
        IOptions<RabbitMqSettings> settings,
        IHubContext<NotificationHub> hubContext,
        IServiceProvider serviceProvider,
        ILogger<RabbitMqPetEventSubscriber> logger)
    {
        _settings = settings.Value;
        _hubContext = hubContext;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };

        _connection = await factory.CreateConnectionAsync(stoppingToken).ConfigureAwait(false);
        _channel = await _connection.CreateChannelAsync(
            new CreateChannelOptions(false, false, null, null),
            stoppingToken).ConfigureAwait(false);

        await _channel.ExchangeDeclareAsync(
            _settings.ExchangeName,
            ExchangeType.Fanout,
            durable: true,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: stoppingToken).ConfigureAwait(false);
        await _channel.QueueDeclareAsync(
            _settings.QueueName,
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null,
            passive: false,
            noWait: false,
            cancellationToken: stoppingToken).ConfigureAwait(false);
        await _channel.QueueBindAsync(
            _settings.QueueName,
            _settings.ExchangeName,
            string.Empty,
            arguments: null,
            noWait: false,
            cancellationToken: stoppingToken).ConfigureAwait(false);

        var consumer = new AsyncEventingBasicConsumer(_channel);
        consumer.ReceivedAsync += OnEventReceivedAsync;

        await _channel.BasicConsumeAsync(
            _settings.QueueName,
            autoAck: true,
            consumer,
            cancellationToken: stoppingToken).ConfigureAwait(false);

        _logger.LogInformation("RabbitMQ subscriber listening on exchange {Exchange} queue {Queue}", _settings.ExchangeName, _settings.QueueName);

        await Task.Delay(Timeout.Infinite, stoppingToken).ConfigureAwait(false);
    }

    private async Task OnEventReceivedAsync(object sender, BasicDeliverEventArgs args)
    {
        try
        {
            var payload = Encoding.UTF8.GetString(args.Body.ToArray());
            var notificationEvent = JsonSerializer.Deserialize<IntegrationEvent>(payload);

            if (notificationEvent == null || string.IsNullOrWhiteSpace(notificationEvent.Event))
            {
                _logger.LogWarning("Se recibio un evento invalido en RabbitMQ: {Payload}", payload);
                return;
            }

            _logger.LogInformation("Evento recibido: {Event}", notificationEvent.Event);

            // Persistir notificaciones en la base de datos
            await PersistNotificationAsync(notificationEvent);

            // Enviar notificación en tiempo real via SignalR
            if (notificationEvent.Event.Equals("AdoptionStatusChanged", StringComparison.OrdinalIgnoreCase)
                && TryGetTargetUserId(notificationEvent, out var targetUserId)
                && !string.IsNullOrWhiteSpace(targetUserId))
            {
                await _hubContext.Clients.User(targetUserId)
                    .SendAsync(notificationEvent.Event, notificationEvent.Data);
                return;
            }

            await _hubContext.Clients.All.SendAsync(notificationEvent.Event, notificationEvent.Data);
        }
        catch (JsonException jsonException)
        {
            _logger.LogError(jsonException, "Error al deserializar evento de RabbitMQ");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al procesar evento RabbitMQ");
        }
    }

    private async Task PersistNotificationAsync(IntegrationEvent notificationEvent)
    {
        using var scope = _serviceProvider.CreateScope();
        var notificationService = scope.ServiceProvider.GetRequiredService<INotificationService>();
        var userService = scope.ServiceProvider.GetRequiredService<IUserProfileService>();

        try
        {
            if (notificationEvent.Event.Equals("PetCreated", StringComparison.OrdinalIgnoreCase))
            {
                // Obtener nombre de la mascota
                var petName = "Una nueva mascota";
                if (notificationEvent.Data.HasValue && 
                    notificationEvent.Data.Value.TryGetProperty("Pet", out var petObj) &&
                    petObj.TryGetProperty("name", out var nameProperty))
                {
                    petName = nameProperty.GetString() ?? petName;
                }

                // Crear notificación para todos los usuarios
                var allUsers = await userService.GetAllUsersAsync();
                foreach (var user in allUsers)
                {
                    var notification = Notification.Create(
                        user.Id,
                        "NEW_PET",
                        $"{petName} está disponible para adopción"
                    );
                    await notificationService.AddNotificationAsync(notification);
                }
                _logger.LogInformation("Notificaciones de nueva mascota creadas para {Count} usuarios", allUsers.Count());
            }
            else if (notificationEvent.Event.Equals("AdoptionStatusChanged", StringComparison.OrdinalIgnoreCase))
            {
                // Notificar al usuario específico
                if (TryGetTargetUserId(notificationEvent, out var userId) && 
                    int.TryParse(userId, out var userIdInt))
                {
                    var status = "actualizado";
                    if (notificationEvent.Data.HasValue && 
                        notificationEvent.Data.Value.TryGetProperty("AdoptionRequest", out var requestObj) &&
                        requestObj.TryGetProperty("status", out var statusProperty))
                    {
                        status = statusProperty.GetString() ?? status;
                    }

                    var notification = Notification.Create(
                        userIdInt,
                        "ADOPTION_STATUS",
                        $"Tu solicitud de adopción ha sido {status}"
                    );
                    await notificationService.AddNotificationAsync(notification);
                    _logger.LogInformation("Notificación de cambio de estado creada para usuario {UserId}", userIdInt);
                }
            }
            else if (notificationEvent.Event.Equals("AdoptionRequestCreated", StringComparison.OrdinalIgnoreCase))
            {
                // Notificar solo a los administradores
                var admins = await userService.GetUsersByRoleAsync("Admin");
                
                foreach (var admin in admins)
                {
                    var notification = Notification.Create(
                        admin.Id,
                        "NEW_REQUEST",
                        "Hay una nueva solicitud de adopción pendiente"
                    );
                    await notificationService.AddNotificationAsync(notification);
                }
                _logger.LogInformation("Notificaciones de nueva solicitud creadas para {Count} administradores", admins.Count());
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error al persistir notificación para evento {Event}", notificationEvent.Event);
        }
    }

    public override Task StopAsync(CancellationToken cancellationToken)
    {
        (_channel as IDisposable)?.Dispose();
        (_connection as IDisposable)?.Dispose();
        return base.StopAsync(cancellationToken);
    }

    private static bool TryGetTargetUserId(IntegrationEvent notificationEvent, out string? targetUserId)
    {
        targetUserId = null;

        if (notificationEvent.Data is null)
            return false;

        var data = notificationEvent.Data.Value;
        if (data.ValueKind != JsonValueKind.Object)
            return false;

        if (data.TryGetProperty("TargetUserId", out var explicitTarget))
        {
            targetUserId = GetJsonValueAsString(explicitTarget);
            return !string.IsNullOrWhiteSpace(targetUserId);
        }

        if (data.TryGetProperty("AdoptionRequest", out var adoptionRequest)
            && adoptionRequest.ValueKind == JsonValueKind.Object
            && adoptionRequest.TryGetProperty("UserId", out var userIdProperty))
        {
            targetUserId = GetJsonValueAsString(userIdProperty);
            return !string.IsNullOrWhiteSpace(targetUserId);
        }

        return false;
    }

    private static string? GetJsonValueAsString(JsonElement element)
    {
        return element.ValueKind switch
        {
            JsonValueKind.String => element.GetString(),
            JsonValueKind.Number when element.TryGetInt32(out var value) => value.ToString(),
            _ => element.ToString()
        };
    }
}
