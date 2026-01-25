// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Lite.EventIpc;
////using Tmds.DBus;
using Tmds.DBus.Protocol;

namespace Sample.IpcDBus;

public class Publisher
{
  public async Task StartAsync()
  {
    Console.WriteLine("Publisher starting…");

    // 1) Local aggregator
    var aggregator = new EventAggregator();

    // 2) DBus connection (session bus is typical for desktop apps)
    using var connection = new Connection(Address.Session!);
    //// using var connection = new Connection(DBusAddress.Session!);
    //// await using var connection = new Connection(DBusAddress.Session!);

    await connection.ConnectAsync();

    // Try to own a well-known name so others can filter on it
    // (Not strictly required to emit signals, but helpful.)
    await connection.RequestNameAsync(Constants.Service);

    // 3) Publish locally AND over DBus
    var evt = new UserCreatedEvent { UserName = "Damian" };
    aggregator.Publish(evt);

    await EmitDbusSignalAsync(connection, evt);

    Console.WriteLine("Event published locally and emitted over D‑Bus.");
    Console.WriteLine("Press <Enter> to exit.");
    Console.ReadLine();
  }

  private async Task EmitDbusSignalAsync<T>(Connection connection, T evt)
  {
    var eventType = typeof(T).FullName ?? typeof(T).Name;
    var json = JsonSerializer.Serialize(evt);

    using var writer = connection.GetMessageWriter();
    // Signal header: destination(null=broadcast), path, interface, member, signature
    writer.WriteSignalHeader(destination: null,
                             path: Constants.Path,
                             @interface: Constants.Interface,
                             member: Constants.Signal,
                             signature: "ss"); // two strings

    writer.WriteString(eventType);
    writer.WriteString(json);

    var message = writer.CreateMessage();
    connection.TrySendMessage(message);
    ////await connection.SendMessageAsync(message);
  }
}
