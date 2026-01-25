// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

using Lite.EventIpc;
using Tmds.DBus.Protocol;

namespace Sample.IpcDBus;

public class Subscriber
{
  public static async Task StartSubscriberAsync()
  {
    Console.WriteLine("Subscriber starting…");

    var aggregator = new EventAggregator();

    // Example: subscribe locally (app-internal)
    aggregator.Subscribe<UserCreatedEvent>(e =>
    {
      Console.WriteLine($"[LOCAL] UserCreatedEvent: {e.UserName} at {e.CreatedAtUtc:o}");
    });

    // Connect to the session bus and start watching the signal.
    using var connection = new Tmds.DBus.Connection(Address.Session);
    ////await using var connection = new Tmds.DBus.Connection(DBusAddress.Session!);
    await connection.ConnectAsync();

    // Start a generic watcher for our interface + signal name.
    // This mimics the generated WatchXxxAsync helpers.
    /*
    await WatchEventPublishedAsync(connection, (eventType, json) =>
    {
      // very basic type map - expand as needed, or include type name in payload
      if (eventType == typeof(UserCreatedEvent).FullName)
      {
        var evt = JsonSerializer.Deserialize<UserCreatedEvent>(json);
        if (evt != null)
        {
          Console.WriteLine($"[DBUS] Received {eventType} for user {evt.UserName}");
          aggregator.Publish(evt);
        }
      }
      else
      {
        Console.WriteLine($"[DBUS] Ignoring unknown event type: {eventType}");
      }
    });
    */

    Console.WriteLine("Listening for D‑Bus events… Press <Enter> to exit.");
    Console.ReadLine();
  }

  private static Task<IDisposable> WatchEventPublishedAsync(
          Connection connection,
          Action<(string eventType, string json)> handler,
          Action<Exception>? onError = null)
  {
    throw new NotImplementedException();

    // Behind the scenes, Tmds.DBus generated code uses a WatchSignalAsync pattern like:
    // base.WatchSignalAsync(destination, interface, path, member, reader, handler)
    // We’ll implement a small reader for the 'ss' signature (two strings).
    /*
    return connection.WatchSignalAsync(
        destination: null, // any
        @interface: DbusConstants.Interface,
        path: DbusConstants.Path,
        member: DbusConstants.Signal,
        (Message m, object? _) =>
        {
          var r = m.GetBodyReader();
          string type = r.ReadString();
          string json = r.ReadString();
          return (type, json);
        },
        state: null,
        handler: handler,
        onError: onError,
        emitOnCapturedContext: true);
    */
  }
}
