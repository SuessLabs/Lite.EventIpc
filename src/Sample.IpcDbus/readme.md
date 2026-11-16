# Lite.EventIpc with DBus

The test app utilizes `Tmds.DBus.Protocol` for communication via DBus.

NOTE:

* You must run the test app on either Linux or WSL
* Ensure that the D-Bus is installed/running

```txt
sudo service dbus start         :: Start the service
echo $DBUS_SESSION_BUS_ADDRESS  :: DBUS_SESSION_BUS_ADDRESS environment variable is set.
```

This app was tested using WSL Ubuntu 24.02 LTS on Windows.

## Session Address

DBus requires an address to connect to. This test app uses, `Address.Session` by default. When running on a system without DBus, you will get a NULL value and it won't work.

```cs
var connection = new Connection(Address.Session!);
```
