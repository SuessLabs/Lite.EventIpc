// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

using Tmds.DBus.Protocol;

namespace Sample.IpcDBus;

/// <summary>
///   Basic example of creating a DBus server connection and adding a proxy.
/// </summary>
public static class BasicDBus
{
  public static async Task RunAsync()
  {
    string peerName = await StartServiceAsync();

    var connection = Connection.Session;

    var addProxy = new AddProxy(connection, peerName);

    int i = 10;
    int j = 20;
    int sum = await addProxy.AddAsync(i, j);

    Console.WriteLine($"The sum of {i} and {j} is {sum}.");
  }

  /// <summary>Starts connection service to listen for messages.</summary>
  /// <returns>DBus connection name.</returns>
  private static async Task<string> StartServiceAsync()
  {
    // NOTE: You must either run on Linux or WSL
    // Also ensure that the D-Bus session bus is running and the
    //  sudo service dbus start         :: Start the service
    //  echo $DBUS_SESSION_BUS_ADDRESS  :: DBUS_SESSION_BUS_ADDRESS environment variable is set.
    var connection = new Connection(Address.Session!);
    ////var connection = new Connection(DBusAddress.Session!);

    await connection.ConnectAsync();

    connection.AddMethodHandler(new AddImplementation());

    return connection.UniqueName ?? string.Empty;
  }

  /// <summary>Service's Custom Path Method Handler.</summary>
  private class AddImplementation : IPathMethodHandler
  {
    // Interface path called by clients
    private const string Interface = "org.example.Adder";

    public bool HandlesChildPaths => false;

    public string Path => "/org/example/Adder";

    private static ReadOnlyMemory<byte> InterfaceXml { get; } =
      """
      <interface name="org.example.Adder">
        <method name="Add">
          <arg direction="in" type="i"/>
          <arg direction="in" type="i"/>
          <arg direction="out" type="i"/>
        </method>
      </interface>

      """u8.ToArray();

    public ValueTask HandleMethodAsync(MethodContext context)
    {
      if (context.IsDBusIntrospectRequest)
      {
        context.ReplyIntrospectXml([InterfaceXml]);
        return default;
      }

      var request = context.Request;
      switch (request.InterfaceAsString)
      {
        case Interface:
          switch ((request.MemberAsString, request.SignatureAsString))
          {
            case ("Add", "ii"):
              var reader = request.GetBodyReader();
              int i = reader.ReadInt32();
              int j = reader.ReadInt32();
              return Add(context, i, j);
          }

          break;
      }

      return default;
    }

    private ValueTask Add(MethodContext context, int i, int j)
    {
      // The handling may be done as part of this method as shown here.
      // Or it may run asynchronous to the method, as shown in the commented out code.
      int sum = i + j;
      ReplyToAdd(context, sum);
      return default;

      // For async handling, set DisposesAsynchronously and dispose the context when async handling is complete.
      //   _ = Task.Run(async () =>
      //  {
      //    using (context)
      //    {
      //      try
      //      {
      //        int sum = i + j;
      //        await Task.Delay(100);
      //        ReplyToAdd(context, sum);
      //      }
      //      catch (Exception ex)
      //      {
      //        // Handle the exception, for example by disconnecting or replying with an error.
      //        // context.Disconnect(ex);
      //        // context.ReplyError(...)
      //      }
      //    }
      //  });
      //  context.DisposesAsynchronously = true;
      //  return default;
    }

    private void ReplyToAdd(MethodContext context, int sum)
    {
      using var writer = context.CreateReplyWriter("i");
      writer.WriteInt32(sum);
      context.Reply(writer.CreateMessage());
    }
  }

  /// <summary>Client proxy to call our listener.</summary>
  private class AddProxy
  {
    private const string Interface = "org.example.Adder";
    private const string Path = "/org/example/Adder";

    private readonly Connection _connection;
    private readonly string _peer;

    public AddProxy(Connection connection, string peer)
    {
      _connection = connection;
      _peer = peer;
    }

    public Task<int> AddAsync(int i, int j)
    {
      return _connection.CallMethodAsync(
        CreateAddMessage(),
        (Message message, object? state) =>
        {
          return message.GetBodyReader().ReadInt32();
        });

      MessageBuffer CreateAddMessage()
      {
        using var writer = _connection.GetMessageWriter();

        writer.WriteMethodCallHeader(
          destination: _peer,
          path: Path,
          @interface: Interface,
          signature: "ii",
          member: "Add");         // Name of method to call

        writer.WriteInt32(i);
        writer.WriteInt32(j);

        return writer.CreateMessage();
      }
    }
  }
}
