// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;

namespace Lite.EventIpc.Tests;

[TestClass]
[System.Diagnostics.CodeAnalysis.SuppressMessage("Design", "MSTEST0016:Test class should have test method", Justification = "Its a base class")]
public class BaseTestClass
{
  /// <summary>Timeout 200 milliseconds.</summary>
  public const int Timeout200 = 200;

  /// <summary>Timeout 50 milliseconds.</summary>
  public const int Timeout50 = 50;

  [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.MaintainabilityRules", "SA1401:Fields should be private", Justification = "This is a parent class")]
  protected ILogger<EventAggregator> _logger;

  public BaseTestClass()
  {
    _logger = CreateConsoleLogger<EventAggregator>(LogLevel.Trace);
  }

  public TestContext TestContext { get; set; }

  protected static ILogger<T> CreateConsoleLogger<T>(LogLevel minimumLevel = LogLevel.Debug)
  {
    var factory = LoggerFactory.Create(config =>
    {
      //// config.AddConsole();
      config.AddSimpleConsole(options =>
      {
        options.TimestampFormat = "HH:mm:ss.fff ";
        options.UseUtcTimestamp = false;
        options.IncludeScopes = true;
        options.SingleLine = true;
        options.ColorBehavior = LoggerColorBehavior.Enabled;
      });
      config.SetMinimumLevel(minimumLevel);
    });

    return factory.CreateLogger<T>();
  }
}
