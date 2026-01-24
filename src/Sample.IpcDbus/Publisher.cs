// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

using System.Text.Json;
using Lite.EventIpc;
using Tmds.DBus;

namespace Sample.IpcDBus;

public class Publisher
{
  public async Task StartAsync()
  {
    Console.WriteLine("Publisher starting…");

    // 1) Local aggregator
    var aggregator = new EventAggregator();

    // 2) DBus connection (session bus is typical for desktop apps)
    using var connection = new Connection(DbusConstants.Path!);
    //// await using var connection = new Connection(DBusAddress.Session!);
    await connection.ConnectAsync();

    // Try to own a well-known name so others can filter on it
    // (Not strictly required to emit signals, but helpful.)
    await connection.RequestNameAsync(DbusConstants.Service);

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
                             path: DbusConstants.Path,
                             @interface: DbusConstants.Interface,
                             member: DbusConstants.Signal,
                             signature: "ss"); // two strings

    writer.WriteString(eventType);
    writer.WriteString(json);

    var message = writer.CreateMessage();
    await connection.SendMessageAsync(message);
  }
}
