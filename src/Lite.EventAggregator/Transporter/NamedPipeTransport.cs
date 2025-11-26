// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;

namespace Lite.EventAggregator.Transporter;

public class NamedPipeTransport : IEventTransport
{
  private readonly string _pipeName;

  public NamedPipeTransport(string pipeName)
  {
    _pipeName = pipeName;
  }

  public void Send<TEvent>(TEvent eventData)
  {
    // Serialize and write to named pipe
  }

  public void StartListening<TEvent>(Action<TEvent> onEventReceived)
  {
    // Read from named pipe and deserialize
  }
}
