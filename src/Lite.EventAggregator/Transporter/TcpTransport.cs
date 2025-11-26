// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

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
    var json = EventSerializer.Serialize(eventData);
    using var client = new TcpClient(_host, _port);

    var stream = client.GetStream();
    var bytes = Encoding.UTF8.GetBytes(json);

    stream.Write(bytes, 0, bytes.Length);
  }

  public void StartListening<TEvent>(Action<TEvent> onEventReceived)
  {
    // Listen on TCP socket and deserialize
    Task.Run(() =>
    {
      var listener = new TcpListener(IPAddress.Any, _port);
      listener.Start();
      while (true)
      {
        using var client = listener.AcceptTcpClient();
        using var stream = client.GetStream();

        var buffer = new byte[4096];
        var bytesRead = stream.Read(buffer, 0, buffer.Length);

        var json = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        var evt = EventSerializer.Deserialize<TEvent>(json);

        onEventReceived(evt);
      }
    });
  }
}
