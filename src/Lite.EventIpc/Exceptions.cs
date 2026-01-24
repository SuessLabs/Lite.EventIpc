// Copyright Xeno Innovations, Inc. 2025
// See the LICENSE file in the project root for more information.

using System;

namespace Lite.EventIpc;

#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable SA1402 // File may only contain a single type

/// <summary>Missing local and IPC subscriptions and not timeout is configured. Blackhole warning.</summary>
/// <param name="message">Message.</param>
public class MissingSubscribeRequestException(string message) : Exception(message);

/// <summary>IPC Transport is not set and UseIpcTransport is enabled.</summary>
/// <param name="message">Message.</param>
public class IpcTransportNotSetException(string message) : InvalidOperationException(message);

#pragma warning restore SA1402 // File may only contain a single type
#pragma warning restore SA1649 // File name should match first type name
