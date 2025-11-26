// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Text;

namespace Lite.EventAggregator.Transporter;

public class MemoryMappedTransport : IEventTransport
{
  public void Send<TEvent>(TEvent eventData)
  {
    // Serialize and write to memory-mapped file
  }

  public void StartListening<TEvent>(Action<TEvent> onEventReceived)
  {
    // Poll memory-mapped file for changes
  }
}
