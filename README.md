# Onomondo.ApiClient

An unofficial .NET API client for [Onomondo](https://onomondo.com), a global IoT connectivity platform. This library provides a strongly-typed, modern C# interface to interact with the Onomondo API.

## Features

- **SIM Management**: List, retrieve, and update SIM cards
- **Tag Management**: Organize SIMs with tags
- **Real-time Traffic Monitoring**: Subscribe to live packet capture events via WebSocket
- **Address Range Utilities**: Helper methods to identify Onomondo IP ranges
- **Pagination Support**: Automatic cursor-based pagination using async enumerables
- **AOT Compatible**: Supports Native AOT compilation for optimal performance

## Installation

```bash
dotnet add package Onomondo.ApiClient
```

## Quick Start

### Configuration with Dependency Injection

```csharp
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();

services.AddOnomondoApiClient(options =>
{
    options.ApiKey = "your-api-key-here";
});

var serviceProvider = services.BuildServiceProvider();
var client = serviceProvider.GetRequiredService<IOnomondoApiClient>();
```

## Usage Examples

### Working with SIMs

#### List All SIMs

```csharp
// Get all SIMs using async enumerable with automatic pagination
await foreach (var sim in client.Sims.EnumerateSimsAsync())
{
    Console.WriteLine($"SIM {sim.Iccid}: {sim.Label}");
    Console.WriteLine($"  IP: {sim.Ipv4}");
    Console.WriteLine($"  Online: {sim.Online}");
}

// Or manually control pagination
var page = await client.Sims.ListSimsAsync(limit: 50);
foreach (var sim in page.Sims)
{
    Console.WriteLine($"SIM: {sim.Iccid}");
}
```

#### Manage Tags on SIMs

```csharp
// Add a tag to a SIM
await client.Sims.AddTagAsync("sim-id", "tag-id");

// Remove a tag from a SIM
await client.Sims.RemoveTagAsync("sim-id", "tag-id");

// Set a tag (adds only if not already present)
bool wasAdded = await client.Sims.SetTagAsync("sim-id", "tag-id");
```

### Real-time Traffic Monitoring

Monitor live packet capture events from your SIMs:

```csharp
using Onomondo.ApiClient;

var monitor = new TrafficMonitor("your-api-key-here");

// Connect to the WebSocket
await monitor.ConnectAsync();

// Subscribe to packets from a specific SIM
var subscription = await monitor.SubscribeAsync("sim-id");

// Process captured packets
await foreach (var packet in subscription)
{
    Console.WriteLine($"Packet from {packet.SimId}");
    Console.WriteLine($"  IP: {packet.SimIp}");
    Console.WriteLine($"  Size: {packet.PacketBytes.Length} bytes");
    Console.WriteLine($"  Timestamp: {packet.Timestamp}");
    
    // Process raw packet bytes
    AnalyzePacket(packet.PacketBytes);
}

// Unsubscribe when done
await monitor.UnsubscribePacketsAsync("sim-id");

// Disconnect
await monitor.DisconnectAsync();
```

#### Multiple SIM Monitoring

```csharp
// Subscribe to multiple SIMs simultaneously
var subscription1 = await monitor.SubscribeAsync(["sim-id-1", "sim-id-2"]);

// Process captured packets
await foreach (var packet in subscription)
{
    // Packet from either sim
    Console.WriteLine($"Packet from {packet.SimId}");
}
```

### IP Address Range Utilities

Verify if an IP address belongs to Onomondo's infrastructure:

```csharp
using System.Net;
using Onomondo.ApiClient;

var ipAddress = IPAddress.Parse("185.228.69.100");

// Check if IP is from a SIM
if (OnomondoAddressRanges.IsSimAddress(ipAddress))
{
    Console.WriteLine("This IP belongs to an Onomondo SIM");
}

// Check if IP is from a webhook source
if (OnomondoAddressRanges.IsWebhookAddress(ipAddress))
{
    Console.WriteLine("This is a valid Onomondo webhook source");
}
```

## Error Handling

The client uses `ApiException` from Refit for HTTP errors and `OnomondoException` for traffic monitoring errors.

## License

MIT

## Links

- [Onomondo Website](https://onomondo.com)
- [Onomondo API Documentation](https://docs.onomondo.com)

## Disclaimer

This is an **unofficial** client library and is not affiliated with or endorsed by Onomondo. Use at your own risk.
