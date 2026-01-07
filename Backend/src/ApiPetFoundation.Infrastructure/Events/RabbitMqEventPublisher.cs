using System.Text.Json;
using ApiPetFoundation.Application.Events;
using ApiPetFoundation.Application.Interfaces.Services;
using ApiPetFoundation.Infrastructure.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RabbitMQ.Client;

namespace ApiPetFoundation.Infrastructure.Events;

public class RabbitMqEventPublisher : IEventPublisher, IDisposable
{
    private readonly RabbitMqSettings _settings;
    private readonly ILogger<RabbitMqEventPublisher> _logger;
    private readonly ConnectionFactory _factory;
    private IConnection? _connection;
    private IChannel? _channel;
    private readonly SemaphoreSlim _initLock = new(1, 1);
    private readonly SemaphoreSlim _exchangeDeclareLock = new(1, 1);
    private bool _exchangeDeclared;

    public RabbitMqEventPublisher(
        IOptions<RabbitMqSettings> settings,
        ILogger<RabbitMqEventPublisher> logger)
    {
        _settings = settings.Value;
        _logger = logger;

        _factory = new ConnectionFactory
        {
            HostName = _settings.HostName,
            Port = _settings.Port,
            UserName = _settings.UserName,
            Password = _settings.Password
        };
    }

    public async Task PublishAsync(string eventName, object? payload)
    {
        var channel = await EnsureChannelAsync().ConfigureAwait(false);

        if (!_exchangeDeclared)
        {
            await _exchangeDeclareLock.WaitAsync().ConfigureAwait(false);
            try
            {
                if (!_exchangeDeclared)
                {
                    await channel.ExchangeDeclareAsync(
                        _settings.ExchangeName,
                        ExchangeType.Fanout,
                        durable: true,
                        autoDelete: false,
                        arguments: null,
                        passive: false,
                        noWait: false,
                        cancellationToken: CancellationToken.None).ConfigureAwait(false);
                    _exchangeDeclared = true;
                }
            }
            finally
            {
                _exchangeDeclareLock.Release();
            }
        }

        var integrationEvent = new IntegrationEvent
        {
            Event = eventName,
            Data = ToJsonElement(payload),
            OccurredAt = DateTime.UtcNow,
            Version = 1
        };

        var body = JsonSerializer.SerializeToUtf8Bytes(integrationEvent);
        var props = new BasicProperties { Persistent = true };

        await channel.BasicPublishAsync(
            _settings.ExchangeName,
            string.Empty,
            mandatory: false,
            basicProperties: props,
            body: body,
            cancellationToken: CancellationToken.None).ConfigureAwait(false);
        _logger.LogDebug("Published event {Event} to exchange {Exchange}", eventName, _settings.ExchangeName);
    }

    private async Task<IChannel> EnsureChannelAsync()
    {
        if (_channel != null)
            return _channel;

        await _initLock.WaitAsync().ConfigureAwait(false);
        try
        {
            if (_channel == null)
            {
                _connection = await _factory.CreateConnectionAsync(CancellationToken.None).ConfigureAwait(false);
                _channel = await _connection.CreateChannelAsync(
                    new CreateChannelOptions(false, false, null, null),
                    CancellationToken.None)
                    .ConfigureAwait(false);
            }
        }
        finally
        {
            _initLock.Release();
        }

        return _channel;
    }

    private static JsonElement? ToJsonElement(object? payload)
    {
        if (payload == null)
            return null;

        var json = JsonSerializer.Serialize(payload);
        using var document = JsonDocument.Parse(json);
        return document.RootElement.Clone();
    }

    public void Dispose()
    {
        (_channel as IDisposable)?.Dispose();
        (_connection as IDisposable)?.Dispose();
        _initLock.Dispose();
        _exchangeDeclareLock.Dispose();
    }
}
