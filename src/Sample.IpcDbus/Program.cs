// Copyright Xeno Innovations, Inc. 2026
// See the LICENSE file in the project root for more information.

namespace Sample.IpcDBus;

public class Program
{
  [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles", Justification = "main entry point")]
  public static async Task Main(string[] args)
  {
    // Run the quick test example
    await BasicDBus.RunAsync();
  }
}
