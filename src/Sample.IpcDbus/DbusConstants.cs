// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

namespace Sample.IpcDBus;

// Choose the bus: Session for user apps, System for machine-wide.
public static class DbusConstants
{
  public const string Interface = "com.suesslabs.LiteEventIpc.Events";
  public const string Path = "/com/suesslabs/LiteEventIpc";
  public const string Service = "com.suesslabs.LiteEventIpc";
  public const string Signal = "EventPublished";
}
