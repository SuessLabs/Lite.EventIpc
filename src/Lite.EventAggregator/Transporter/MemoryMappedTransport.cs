// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.IO.MemoryMappedFiles;
using System.Runtime.Versioning;
using System.Text;
using System.Threading.Tasks;

namespace Lite.EventAggregator.Transporter;

/// <summary>Memory-Mapped IPC Transport (Windows OS only).</summary>
[SupportedOSPlatform("windows")]
public class MemoryMappedTransport : IEventTransport
{
  private const int BufferSize = 4096;
  private readonly string _mapName;

  public MemoryMappedTransport(string mapName)
  {
    _mapName = mapName;
  }

  public void Send<TEvent>(TEvent eventData)
  {
    if (_mapName is null)
      throw new InvalidOperationException("Missing map name");

    // Serialize and write to memory-mapped file
    var json = EventSerializer.Serialize(eventData);
    using var mmf = MemoryMappedFile.CreateOrOpen(_mapName, BufferSize);
    using var accessor = mmf.CreateViewAccessor();

    var bytes = Encoding.UTF8.GetBytes(json);
    accessor.WriteArray(0, bytes, 0, bytes.Length);
  }

  public void StartListening<TEvent>(Action<TEvent> onEventReceived)
  {
    if (_mapName is null)
      throw new InvalidOperationException("Missing map name");

    // Poll memory-mapped file for changes
    Task.Run(() =>
    {
      using var mmf = MemoryMappedFile.OpenExisting(_mapName);
      using var accessor = mmf.CreateViewAccessor();
      var bytes = new byte[BufferSize];
      accessor.ReadArray(0, bytes, 0, bytes.Length);

      var json = Encoding.UTF8.GetString(bytes).TrimEnd('\0');
      var evt = EventSerializer.Deserialize<TEvent>(json);
      onEventReceived(evt);
    });
  }
}
