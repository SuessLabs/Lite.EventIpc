# Testing D-Bus from Windows

Use WSL 2 with Ubuntu to run `Sample.IpcDbus` against a real Linux D-Bus session
on your Windows machine. This sample is in the adjacent `SuessLabs.Lite.EventIpc`
repository.

## 1. Install WSL

Open PowerShell **as Administrator** and run:

```powershell
wsl --install -d Ubuntu-24.04
```

Restart if prompted, then open Ubuntu and create its Linux username and password.

See [Microsoft's WSL installation instructions](https://learn.microsoft.com/en-us/windows/wsl/install).

## 2. Install .NET and D-Bus inside Ubuntu

Run these commands in the Ubuntu terminal:

```bash
sudo apt update
sudo apt install -y dotnet-sdk-10.0 dbus dbus-daemon
dotnet --list-sdks
```

Confirm that a `10.0` SDK appears. The Linux SDK is installed separately from your
Windows SDK. Use `dotnet` inside Ubuntu, rather than the Windows `dotnet.exe`.

See [Install .NET on Ubuntu 24.04](https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu-install#ubuntu-2404).

## 3. Build the sample using Linux .NET

The following commands assume the repository is at
`C:\dev\SuessLabs.Lite.EventIpc`. Adjust the path if needed.

```bash
cd /mnt/c/dev/SuessLabs.Lite.EventIpc
dotnet build src/Sample.IpcDbus/Sample.IpcDBus.csproj
```

## 4. Run inside an isolated D-Bus session

From the same Ubuntu terminal and repository directory, run:

```bash
dbus-run-session -- dotnet run \
  --project src/Sample.IpcDbus/Sample.IpcDBus.csproj \
  --no-build
```

`dbus-run-session` starts a session bus, sets `DBUS_SESSION_BUS_ADDRESS` for the
application, and stops the bus when the application exits. You do not need to
start a system service manually or set the address yourself.

See the [dbus-run-session documentation](https://dbus.freedesktop.org/doc/dbus-run-session.1.html).

## 5. Check the result

Expected output:

```text
The sum of 10 and 20 is 30.
```

This confirms that the basic example completed its D-Bus method request and
reply: the client sent two integers, the service added them, and the client
received the result.

## Advanced publisher/subscriber limitation

These steps test the basic example invoked by `Program.Main`. The advanced event
publisher/subscriber example still requires implementation changes: its
subscription call is commented out, and its watcher throws
`NotImplementedException`. Installing WSL does not resolve those code gaps.

## Verification status

The sample built successfully on Windows during review. Running it there without
a session bus failed with `ArgumentNullException` because the bus address was
null. A live Linux D-Bus round trip was not verified during that review because
WSL was not installed.
