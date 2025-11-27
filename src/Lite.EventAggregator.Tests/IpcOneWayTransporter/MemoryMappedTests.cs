// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Versioning;
using Lite.EventAggregator.Transporter;

namespace Lite.EventAggregator.Tests.IpcOneWayTransporter;

[SupportedOSPlatform("windows")]
[TestClass]
public class MemoryMappedTests : BaseTestClass
{
  [TestMethod]
  public void OneWayMemoryMapTest()
  {
    var server = new MemoryMappedTransport("map-name");
    var client = new MemoryMappedTransport("map-name");

    bool msgReceived = false;

    server.StartListening<string>(message =>
    {
      Assert.AreEqual("Hello, Memory Mapped!", message);
      msgReceived = true;
    });

    client.Send(new string("Hello, Memory Mapped!"));

    Assert.IsTrue(msgReceived);
  }

  [TestMethod]
  [Ignore("Not implemented")]
  public void ReceiptedEnvelopeMemoryMapTest()
  {
    var serverTransport = new MemoryMappedEnvelopeTransport(
      requestMapName: "req-map",
      responseMapName: "resp-map",
      requestSignalName: "req-signal",
      responseSignalName: "resp-signal");

    var clientTransport = new MemoryMappedEnvelopeTransport(
      requestMapName: "req-map",
      responseMapName: "resp-map",
      requestSignalName: "req-signal",
      responseSignalName: "resp-signal");
  }
}
