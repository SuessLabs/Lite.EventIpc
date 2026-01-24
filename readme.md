# Lite Event Aggregator and IPC Transporter

<!-- ![Lite.StateMachine Logo](https://raw.githubusercontent.com/SuessLabs/Lite.StateMachine/develop/docs/icon-128x128.png) -->

<image align="right" width="200" height="200" src="https://raw.githubusercontent.com/SuessLabs/Lite.StateMachine/develop/docs/icon-128x128.png" />

Lite.EventIPC is a cross-platform local Event Aggregator and remote IPC (inter-process communication) service library for C#. The pattern is used for decoupling publishers and subscribers in a single or multiple applications. The library can be easily extended for custom IPC transports using the `IEventTransport` interface to suit your needs (_need I say, DBus?_)

The Event Aggregator service in C# pattern is useful for decoupling publishers and subscribers in an application.

||
|-|
| Copyright 2025-2026 Xeno Innovations, Inc. (_DBA: Suess Labs_) |
| Created by: Damian Suess |


## Package Releases

| Package | Stable | Preview
|-|-|-|
| Lite.EventIpc | [![Lite.EventIpc NuGet Badge](https://img.shields.io/nuget/v/Lite.EventIpc)](https://www.nuget.org/packages/Lite.EventIpc/) | [![Lite.EventIpc NuGet Badge](https://img.shields.io/nuget/vpre/Lite.EventIpc)](https://www.nuget.org/packages/Lite.EventIpc/)

## Features

This implementation is features:

* Simple, thread-safe, and async friendly
* Local subscribe/publish event aggregator
  * `Subscribe` and `Publish` - for one-way events
  * `SubscribeRequest` and `RequestAsync` - for publishing with an awaited response (_including timeouts_)
* Uses **weak references** to avoid memory leaks
* Cleans up dead references during `Publish`.
* Prevents memory leaks when subscribers are no longer needed.
* DI-friendly with extensions and hosted service
* Built-in IPC transport mechanisms (_inter-process communication_) with **JSON serialization**:
  * Named Pipe Transport
  * Memory-Mapped File Transport (_Windows OS only_)
  * TCP/IP Transport
* 2 types of IPC communication:
  * One-way publish/subscribe (`IEventTransport`)
  * (_COMING SOON_) Bidirectional request/response with timeouts (`IEventEnvelopeTransport`)

## Usage

### Basic Subscribe/Publish

```cs
using Lite.EventIpc;

public class UserCreatedEvent
{
  public string UserName { get; set; }
}

static void Main()
{
  var eventAggregator = new EventAggregator();

  // Subscribe
  eventAggregator.Subscribe<UserCreatedEvent>(e =>
    Console.WriteLine($"User created: {e.Username}"));

  // Publish
  eventAggregator.Publish(new UserCreatedEvent { Username = "Damian" })
}
```

### Publish Requests with Awaited Responses

Subscribing to a **request** event allows users to respond with a different object. This is useful for RPC-like communication.

```cs
using Lite.EventIpc;

public async Task SubscribeWithResponse()
{
  // Subscribe to Ping events that respond with Pong
  _eventAggregator.SubscribeRequest<Ping, Pong>(EventSubscribe_PingAsync);

  // Send a Ping request event and await the Pong response
  Ping ping = new Ping("HELLO!");
  Pong? response = await _eventAggregator.RequestAsync<Ping, Pong>(ping);

  if (response is not null)
  {
    // Received PONG!
  }
}

private async Task<Pong> EventSubscribe_PingAsync(Ping ping)
{
  var response = "Received PING loud and clear";
  return new Pong(response);
}
```

### Handling Request Event Timeouts

Borrowing from the previous example, you can specify a timeout when sending a request event. If no subscriber responds within the timeout period, `null` is returned.

```cs
public async Task SubscribeWithResponse()
{
  // ...

  // Send a Ping request event and await the Pong response
  Ping ping = new Ping("HELLO!");
  var timeout = TimeSpan.FromMilliseconds(3000);

  Pong? response = null;
  try
  {
    response = await _eventAggregator.RequestAsync<Ping, Pong>(ping, timeout);
  }
  catch (TimeoutException)
  {
    // Handle timeout (no response received in time)
  }

  // ...
}
```

## Architecture

### Why Weak References

If you store strong references to handlers, subscribers will never be collected. Using `WeakReference` ensures that if the subscriber is no longer needed, it can be GC'd.

## History

### v1.1.0

* Option to include `Microsoft.Extensions.Logger` for deep-logging

## Future Considerations

* Send IPC only for specified event types.
* Possibly, filtering or priority-based dispatching.
* Refactor `IEventTransport` to`IIpcEvent` for clarity.
* Refactor `Transporter` namespace to `Ipc` or `IpcTransport` for clarity.
* Refactor `IEventEnvelopeTransport` under `IpcReceipted` namespace for clarity.
* Security: Named Pipes, MMF, and TCP should use proper ACLs / TLS / auth in production
