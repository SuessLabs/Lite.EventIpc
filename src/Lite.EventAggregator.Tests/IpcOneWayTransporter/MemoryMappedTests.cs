// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Lite.EventAggregator.Tests.Models;
using Lite.EventAggregator.Transporter;

namespace Lite.EventAggregator.Tests.IpcOneWayTransporter;

[SupportedOSPlatform("windows")]
[TestClass]
public class MemoryMappedTests : BaseTestClass
{
  [TestMethod]
  public void OneWayMemoryMapTest()
  {
    const string ExpectedUserName = "Hello";

    var server = new MemoryMappedTransport("map-name");
    var client = new MemoryMappedTransport("map-name");

    bool msgReceived = false;

    server.StartListening<UserCreatedEvent>(evt =>
    {
      Assert.AreEqual(ExpectedUserName, evt.UserName);
      msgReceived = true;
    });

    client.Send(new UserCreatedEvent { UserName = ExpectedUserName });

    Task.Delay(10000).Wait();
    server.StopListening();

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
