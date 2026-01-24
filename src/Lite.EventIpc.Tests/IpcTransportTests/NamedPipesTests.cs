// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Lite.EventIpc.IpcTransport;
using Lite.EventIpc.Tests.Models;

namespace Lite.EventIpc.Tests.IpcTransportTests;

[TestClass]
[DoNotParallelize]
public class NamedPipesTests : BaseTestClass
{
  private readonly string _msgPayload = "hello";
  private bool _msgReceived = false;

  [TestInitialize]
  public void CleanupTestInitialize()
  {
    _msgReceived = false;
  }

  [TestMethod]
  [DoNotParallelize]
  public void NamedPipeTest()
  {
    var serverTransport = new NamedPipeTransport("server-requests-in");
    var clientTransport = new NamedPipeTransport("server-requests-in");

    serverTransport.StartListening<Ping>(req =>
    {
      if (req.Message == _msgPayload)
        _msgReceived = true;
    });

    serverTransport.Send<Ping>(new Ping(_msgPayload));

    // Give it a moment
    Task.Delay(Timeout200, TestContext.CancellationTokenSource.Token).Wait(TestContext.CancellationTokenSource.Token);

    Assert.IsTrue(_msgReceived);
  }

  [TestMethod]
  [Ignore("This methodology is not implemented yet.")]
  public void VNextNamedPipeTest()
  {
    var client = new EventAggregator();
    var server = new EventAggregator();

    var serverTransport = new NamedPipeTransport("server-requests-in");
    var clientTransport = new NamedPipeTransport("server-requests-in");

    server.UseIpcTransport(serverTransport);
    client.UseIpcTransport(clientTransport);

    server.Subscribe<Ping>(req =>
    {
      if (req.Message == _msgPayload)
        _msgReceived = true;
    });

    client.Publish<Ping>(new Ping(_msgPayload));

    Assert.IsTrue(_msgReceived);
  }
}
