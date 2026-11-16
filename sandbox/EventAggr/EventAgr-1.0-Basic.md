# Event Aggregator

The Event Aggregator pattern

Here’s a simple implementation of an **Event Aggregator** service in C#. This pattern is useful for decoupling publishers and subscribers in an application.

### Key Features:

*   Allows multiple subscribers to listen for events.
*   Publishers can raise events without knowing who is subscribed.
*   Thread-safe implementation using `ConcurrentDictionary`.

### **C# Implementation**

```csharp
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

public interface IEventAggregator
{
  void Subscribe<TEvent>(Action<TEvent> handler);
  void Unsubscribe<TEvent>(Action<TEvent> handler);
  void Publish<TEvent>(TEvent eventData);
}

public class EventAggregator : IEventAggregator
{
  private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();

  public void Subscribe<TEvent>(Action<TEvent> handler)
  {
    var eventType = typeof(TEvent);
    _subscribers.AddOrUpdate(eventType,
      _ => new List<Delegate> { handler },
      (_, handlers) =>
      {
        handlers.Add(handler);
        return handlers;
      });
  }

  public void Unsubscribe<TEvent>(Action<TEvent> handler)
  {
    var eventType = typeof(TEvent);
    if (_subscribers.TryGetValue(eventType, out var handlers))
    {
      handlers.Remove(handler);
    }
  }

  public void Publish<TEvent>(TEvent eventData)
  {
    var eventType = typeof(TEvent);
    if (_subscribers.TryGetValue(eventType, out var handlers))
    {
      foreach (var handler in handlers)
      {
        (handler as Action<TEvent>)?.Invoke(eventData);
      }
    }
  }
}
```

***

### Usage Example

```csharp
public class UserCreatedEvent
{
  public string Username { get; set; }
}

class Program
{
  static void Main()
  {
    IEventAggregator eventAggregator = new EventAggregator();

    // Subscribe
    eventAggregator.Subscribe<UserCreatedEvent>(e =>
      Console.WriteLine($"User created: {e.Username}"));

    // Publish
    eventAggregator.Publish(new UserCreatedEvent { Username = "Damian" });
  }
}
```

***

✅ This implementation is **simple and thread-safe**, but you can enhance it with:

*   **Weak references** to avoid memory leaks.
*   **Async support** for event handling.
*   **Filtering** or **priority-based dispatch**.

***

Do you want me to **extend this with async support and weak references** for better memory management, or keep it simple? Or should I **wrap this in a .NET DI-friendly service** for real-world usage?
