// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

namespace Lite.EventAggregator;

using System.Text.Json;

public static class EventSerializer
{
  public static T Deserialize<T>(string json) => JsonSerializer.Deserialize<T>(json)!;

  public static string Serialize<T>(T obj) => JsonSerializer.Serialize(obj);
}
