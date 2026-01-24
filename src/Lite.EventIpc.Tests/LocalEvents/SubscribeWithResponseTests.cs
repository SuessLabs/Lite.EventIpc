// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Lite.EventIpc.Tests.Models;
using Microsoft.Extensions.Logging;

namespace Lite.EventIpc.Tests.LocalEvents;

[TestClass]
public class SubscribeWithResponseTests : BaseTestClass
{
  private EventAggregator _eventAggregator;

  public SubscribeWithResponseTests()
  {
    _eventAggregator = null!; // Suppress not initialized warning
  }

  [TestInitialize]
  public void CleanupTestInitialize()
  {
    _logger = CreateConsoleLogger<EventAggregator>(LogLevel.Trace);
    _eventAggregator = new EventAggregator(_logger);
  }

  [TestMethod]
  public async Task LocalEventRequestWithResponseSuccessAsync()
  {
    const string sendMsg = "hi";
    const string recvMsg = "HI";

    _eventAggregator.SubscribeRequest<Ping, Pong>(
      req => Task.FromResult(new Pong(req.Message.ToUpperInvariant())));

    // NOTE: No timeout provided
    var resp = await _eventAggregator.RequestAsync<Ping, Pong>(new Ping(sendMsg));
    Assert.AreEqual(recvMsg, resp.Message);
  }

  [TestMethod]
  public async Task RequestTimeoutsThrowsAsync()
  {
    // Use case:
    //  There is no subscription found so if falls through
    //  This is usually reserved for an IPC receipted event (`_ipcEnvelopeTransport`)
    await Assert.ThrowsAsync<TimeoutException>(async () =>
        await _eventAggregator.RequestAsync<Ping, Pong>(
          new Ping("hi"),
          timeout: TimeSpan.FromMilliseconds(DefaultTimeout),
          System.Threading.CancellationToken.None));
  }

  [TestMethod]
  public async Task RequestWithoutTimeoutsThrowsAsync()
  {
    // Use case:
    //  There is no local subscription, no IPC, or timeout assigned
    await Assert.ThrowsAsync<TimeoutException>(async () =>
        await _eventAggregator.RequestAsync<Ping, Pong>(
          new Ping("hi"),
          timeout: null,
          System.Threading.CancellationToken.None));
  }
}
