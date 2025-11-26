// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lite.EventAggregator;

public class EventAggregator : IEventAggregator
{
  private readonly ConcurrentDictionary<Type, List<Delegate>> _subscribers = new();

  public void Publish<TEvent>(TEvent eventData)
  {
    var eventType = typeof(TEvent);
    if (_subscribers.TryGetValue(eventType, out var handlers))
    {
      foreach (var handler in handlers)
        (handler as Action<TEvent>)?.Invoke(eventData);
    }
  }

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
      handlers.Remove(handler);
  }
}
