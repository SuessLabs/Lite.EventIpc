// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.IO.Pipes;
using System.Text;
using System.Threading.Tasks;

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
    var json = EventSerializer.Serialize(eventData);
    using var client = new NamedPipeClientStream(".", _pipeName, PipeDirection.Out);
    client.Connect();

    var bytes = Encoding.UTF8.GetBytes(json);
    client.Write(bytes, 0, bytes.Length);
  }

  public void StartListening<TEvent>(Action<TEvent> onEventReceived)
  {
    Task.Run(() =>
    {
      using var server = new NamedPipeServerStream(_pipeName, PipeDirection.In);
      server.WaitForConnection();

      using var reader = new StreamReader(server);
      var json = reader.ReadToEnd();

      var evt = EventSerializer.Deserialize<TEvent>(json);
      onEventReceived(evt);
    });
  }
}
