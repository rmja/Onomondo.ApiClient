using System.Collections.Concurrent;
using System.Net;
using System.Text.Json.Serialization;
using System.Threading.Channels;
using Microsoft.Extensions.Logging;
using SocketIOClient;

namespace Onomondo.ApiClient;

public sealed class TrafficMonitor : IAsyncDisposable
{
    private const string ApiUrl = "https://api.onomondo.com";
    private readonly string _apiKey;
    private readonly ILogger? _logger;
    private readonly SocketIO _socket;
    private TaskCompletionSource _authenticatedTcs = new();
    private TaskCompletionSource _disconnectedTcs = new();
    private readonly ConcurrentDictionary<string, Sim> _sims = new();

    public bool Connected => _socket.Connected;
    public Task Disconnected => _disconnectedTcs.Task;

    static class EventNames
    {
        public const string Authenticated = "authenticated";
        public const string SubscribedPackets = "subscribed:packets";
        public const string SubscribeError = "subscribe-error";
        public const string Packets = "packets";
    }

    public TrafficMonitor(string apiKey, ILogger? logger = null)
    {
        var version = typeof(TrafficMonitor).Assembly.GetName().Version!.ToString(3);
        _apiKey = apiKey;
        _logger = logger;
        _socket = new SocketIO(
            new(ApiUrl),
            new SocketIOOptions()
            {
                Path = "/monitor",
                ExtraHeaders = new Dictionary<string, string>()
                {
                    ["user-agent"] = $"Onomondo.Api/{version}",
                },
                Reconnection = false,
            }
        );

        if (logger?.IsEnabled(LogLevel.Debug) == true)
        {
            _socket.OnAny(
                (eventName, response) =>
                {
                    logger.LogDebug(
                        "Got event {EventName} with response {Response}",
                        eventName,
                        response
                    );
                    return Task.CompletedTask;
                }
            );
        }

        if (logger is not null)
        {
            _socket.OnAny(
                (eventName, response) =>
                {
                    if (
                        eventName
                        is not EventNames.Authenticated
                            and not EventNames.SubscribedPackets
                            and not EventNames.SubscribeError
                            and not EventNames.Packets
                    )
                    {
                        logger.LogWarning(
                            "Got unsupported event {EventName} with response {Response}",
                            eventName,
                            response
                        );
                    }
                    return Task.CompletedTask;
                }
            );

            _socket.OnError += (sender, e) =>
            {
                logger.LogError("Socket error: {Message}", e);
            };
        }

        _socket.OnDisconnected += (sender, e) =>
        {
            logger?.LogWarning("Socket disconnected: {Reason}", e);

            var disconnectedException = new OnomondoException("Disconnected");
            _disconnectedTcs.TrySetException(disconnectedException);

            // Propagate disconnection to all active subscriptions
            foreach (var sim in _sims.Values)
            {
                foreach (var subscription in sim.GetSubscriptions())
                {
                    subscription._channel.Writer.TryComplete(disconnectedException);
                }
            }
        };

        _socket.On(
            EventNames.Authenticated,
            (response) =>
            {
                _authenticatedTcs.TrySetResult();
                return Task.CompletedTask;
            }
        );

        _socket.On(
            EventNames.SubscribedPackets,
            (response) =>
            {
                var value = response.GetValue<SubscribedPacketsValue>(0)!;

                var sim = GetSim(value.SimId);
                if (sim is null)
                {
                    return Task.CompletedTask;
                }

                sim.Ip = IPAddress.Parse(value.Ip);
                sim.Attached.SetResult();

                _logger?.LogInformation(
                    "[{SimId}]: Attached with ip {SimIp}",
                    value.SimId,
                    value.Ip
                );
                return Task.CompletedTask;
            }
        );

        // Example response: SIM not found method=subscribe:packet simId=xxx
        _socket.On(
            EventNames.SubscribeError,
            (response) =>
            {
                var message = response.GetValue<string>(0)!;
                var index = message.IndexOf("simId=");
                if (index == -1)
                {
                    _logger?.LogError("Subscribe error with invalid format: {Message}", message);
                    return Task.CompletedTask;
                }

                var simId = message[(index + 6)..];
                var sim = GetSim(simId);
                if (sim is null)
                {
                    return Task.CompletedTask;
                }

                sim.Attached.SetException(new OnomondoException("Subscribe error"));

                _logger?.LogWarning("[{SimId}]: Unable to subscribe to sim", simId);
                return Task.CompletedTask;
            }
        );

        _socket.On(
            EventNames.Packets,
            (response) =>
            {
                var value = response.GetValue<PacketsValue>(0)!;
                var packetBytes = Convert.FromHexString(value.Packet);

                var sim = GetSim(value.SimId);
                if (sim is null)
                {
                    return Task.CompletedTask;
                }

                var packet = new CapturePacket(packetBytes) { SimId = sim.Id, SimIp = sim.Ip };
                sim.PacketReceived(packet);
                return Task.CompletedTask;
            }
        );
    }

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _authenticatedTcs = new(); // Reset for new connection
        _disconnectedTcs = new(); // Reset disconnection state

        await _socket.ConnectAsync(cancellationToken);
        await _socket.EmitAsync("authenticate", [_apiKey], cancellationToken);

        using var cts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        cts.CancelAfter(TimeSpan.FromSeconds(2));

        try
        {
            await _authenticatedTcs.Task.WaitAsync(cts.Token);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            throw new OnomondoException("Unauthorized");
        }
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await _socket.DisconnectAsync();
    }

    public async Task<Subscription> SubscribeAsync(
        string simId,
        CancellationToken cancellationToken = default
    )
    {
        if (!Connected)
        {
            throw new OnomondoException("Not connected");
        }

        var subscription = new Subscription(this);
        var sim = _sims.GetOrAdd(simId, new Sim(simId, _logger));

        sim.AddSubscription(subscription, out int subscribers);
        if (subscribers == 1)
        {
            await _socket.EmitAsync("subscribe:packets", [simId], cancellationToken);
        }

        // Race between attachment, cancellation, and disconnection
        var completedTask = await Task.WhenAny(
            sim.Attached.Task,
            Task.Delay(Timeout.Infinite, cancellationToken),
            _disconnectedTcs.Task
        );

        if (completedTask == _disconnectedTcs.Task)
        {
            await _disconnectedTcs.Task; // Throw disconnection exception
        }

        cancellationToken.ThrowIfCancellationRequested();
        await sim.Attached.Task; // This will throw if subscription failed

        return subscription;
    }

    public async Task<Subscription> SubscribeAsync(
        string[] simIds,
        CancellationToken cancellationToken = default
    )
    {
        if (!Connected)
        {
            throw new OnomondoException("Not connected");
        }

        var subscription = new Subscription(this);
        var tasks = new List<Task>(simIds.Length);
        foreach (var simId in simIds)
        {
            var sim = _sims.GetOrAdd(simId, new Sim(simId, _logger));

            sim.AddSubscription(subscription, out int subscribers);
            if (subscribers == 1)
            {
                await _socket.EmitAsync("subscribe:packets", [simId], cancellationToken);
            }

            tasks.Add(sim.Attached.Task);
        }

        // Race between attachment, cancellation, and disconnection
        var completedTask = await Task.WhenAny(
            Task.WhenAll(tasks),
            Task.Delay(Timeout.Infinite, cancellationToken),
            _disconnectedTcs.Task
        );

        if (completedTask == _disconnectedTcs.Task)
        {
            await _disconnectedTcs.Task; // Throw disconnection exception
        }

        cancellationToken.ThrowIfCancellationRequested();
        foreach (var task in tasks)
        {
            await task; // This will throw if subscription failed
        }

        return subscription;
    }

    private Sim? GetSim(string simId)
    {
        if (!_sims.TryGetValue(simId, out var sim))
        {
            _logger?.LogWarning("[{SimId}]: Sim not found", simId);
            return null;
        }

        return sim;
    }

    public async ValueTask DisposeAsync()
    {
        if (Connected)
        {
            await DisconnectAsync();
        }

        _socket.Dispose();
    }

    internal class Sim(string id, ILogger? logger)
    {
        private readonly HashSet<Subscription> _subscriptions = [];

        public string Id { get; } = id;
        public IPAddress Ip { get; internal set; } = IPAddress.None;
        public TaskCompletionSource Attached { get; internal set; } = new();

        public IEnumerable<Subscription> GetSubscriptions()
        {
            lock (_subscriptions)
            {
                return _subscriptions.ToArray();
            }
        }

        public void AddSubscription(Subscription subscription, out int subscribeCount)
        {
            lock (_subscriptions)
            {
                if (!_subscriptions.Add(subscription))
                {
                    throw new InvalidOperationException(
                        "Cannot to add the same subscription multiple times to the same sim"
                    );
                }

                subscribeCount = _subscriptions.Count;
            }
        }

        public bool TryRemoveSubscription(Subscription subscription, out int subscribeCount)
        {
            lock (_subscriptions)
            {
                if (_subscriptions.Remove(subscription))
                {
                    subscribeCount = _subscriptions.Count;
                    return true;
                }

                subscribeCount = _subscriptions.Count;
                return false;
            }
        }

        public void PacketReceived(CapturePacket packet)
        {
            foreach (var subscription in _subscriptions)
            {
                var written = subscription._channel.Writer.TryWrite(packet);
                if (!written)
                {
                    logger?.LogWarning(
                        "[{SimId}]: Unable to write packet to subscription channel",
                        packet.SimId
                    );
                }
            }
        }

        internal void ClearAttached()
        {
            Attached = new();
        }
    }

    public sealed class Subscription(TrafficMonitor monitor)
        : IAsyncEnumerable<CapturePacket>,
            IAsyncDisposable
    {
        internal readonly Channel<CapturePacket> _channel =
            Channel.CreateUnbounded<CapturePacket>();

        public IAsyncEnumerator<CapturePacket> GetAsyncEnumerator(
            CancellationToken cancellationToken = default
        )
        {
            return _channel
                .Reader.ReadAllAsync(cancellationToken)
                .GetAsyncEnumerator(cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            _channel.Writer.Complete();

            foreach (var sim in monitor._sims.Values)
            {
                if (sim.TryRemoveSubscription(this, out var subscribers))
                {
                    if (subscribers == 0)
                    {
                        sim.ClearAttached();
                        await monitor._socket.EmitAsync("unsubscribe:packets", [sim.Id]);
                    }
                }
            }
        }
    }

    record SubscribedPacketsValue
    {
        [JsonPropertyName("simId")]
        public required string SimId { get; init; }

        [JsonPropertyName("ip")]
        public required string Ip { get; init; }
    }

    record PacketsValue
    {
        [JsonPropertyName("simId")]
        public required string SimId { get; init; }

        [JsonPropertyName("packet")]
        public required string Packet { get; init; }
    }
}
