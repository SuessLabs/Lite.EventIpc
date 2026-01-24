// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

namespace Sample.IpcDBus;

public sealed class UserCreatedEvent
{
  public DateTime CreatedAtUtc { get; init; } = DateTime.UtcNow;

  public string UserName { get; init; } = string.Empty;
}
