// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;

namespace Lite.EventAggregator.Transporter;

public class TcpTransport : IEventTransport
{
  private readonly string _host;
  private readonly int _port;

  public TcpTransport(string host, int port)
  {
    _host = host;
    _port = port;
  }

  public void Send<TEvent>(TEvent eventData)
  {
    // Serialize and send over TCP socket
  }

  public void StartListening<TEvent>(Action<TEvent> onEventReceived)
  {
    // Listen on TCP socket and deserialize
  }
}
