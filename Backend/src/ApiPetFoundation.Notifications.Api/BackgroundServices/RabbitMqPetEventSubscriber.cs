using System.Text;
using System.Text.Json;
using ApiPetFoundation.Application.Events;
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
    private IConnection? _connection;
    private IChannel? _channel;

    public RabbitMqPetEventSubscriber(
        IOptions<RabbitMqSettings> settings,
        IHubContext<NotificationHub> hubContext,
        ILogger<RabbitMqPetEventSubscriber> logger)
    {
        _settings = settings.Value;
        _hubContext = hubContext;
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
