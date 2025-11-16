# Assassin's Creed Brotherhood RDV
All-in-one matchmaking/online service (Quazal's Rendez-Vous, RDV) backend for AC Brotherhood.

This includes:
- `onlineconfigservice.ubi.com` web service
- authentication service
- RDV secure service
- research and debugging tools

The game uses P2P architecture for in-game network traffic.

Credits for the original implementation of [GRO backend](https://github.com/zeroKilo/GROBackendWV) go to [@Warranty Voider](https://github.com/zeroKilo).

## Configuration
Here's the configuration needed to set up your environment.

### Database
As of now the database file is excluded from git tracking. Use the provided script to initialize your local database:

1. Install [SQLite CLI](https://sqlite.org/download.html).

2. Run `db_init.bat` to create and copy database into build directories. It also copies `.cxb` configuration file.

3. (Optional) Install [SQLite Browser](https://sqlitebrowser.org/dl/) for data inspection/modifications.

### Application
Add `appSettings` tag with server IP to `./ACB RDV/App.config` file:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/>
    </startup>
    <appSettings>
        <add key="SecureServerAddress" value="<server host IP>" />
    </appSettings>
</configuration>
```

Set `<server host IP>` to the server host's IP depending on your environment (localhost/LAN/Internet).

Make sure your current build directory was populated with the `.cxb` file.

#### Clients
On every **client** machine add an entry in `C:\Windows\System32\drivers\etc\hosts` to redirect the game's network traffic to the server:
```
<server host IP> onlineconfigservice.ubi.com
```

`<server host IP>` should match that of `ACBRDV.exe.config` for all environment scenarios (localhost/LAN/Internet).

You will likely need admin permissions to save `hosts` file.

## Running and debugging

### Server
Make sure the required ports are available:

| Protocol | Port | Service | Availability |
|---|---|---|---|
| HTTP | 80 | OnlineConfigSvc | Often used by [IIS](https://en.wikipedia.org/wiki/Internet_Information_Services) |
| UDP | 21030 | Quazal auth | Usually available |
| UDP | 21031 | RDV | Usually available |


Make sure you have all required NuGet packages installed.

Build `ACB RDV` project in Visual Studio.

Either:
- run `ACB RDV` project through Visual Studio to debug it
- use `_runme.bat` in the root directory to run the server executable

`./ACB RDV/bin/<architecture>/<mode>/log.txt` contains detailed server log from the last/current run.

### Client
Run this command from ACB root directory to start the game:

```
ACBMP.exe /onlineUser:<user> /onlinePassword:<password>
```

Change `<user>` and `<password>` to your credentials from the database file (any user other than `Tracking`).

To log in as the default player, use:

```
ACBMP.exe /onlineUser:Player /onlinePassword:pass
```

## Contact
You can join our community on [Discord](https://discord.gg/7dpFgtV4uc).

## License
Due to interoperability with proprietary abandonware, this software is licensed under MIT with [Commons Clause](https://commonsclause.com/) (permissive with the exception of sale).

## Disclaimers
This project is not maintained by or affiliated with Ubisoft.

All emulated services and tools were developed using techniques of software reverse engineering, on the basis of Ubisoft's end-of-life [announcement](https://www.ubisoft.com/en-us/help/purchases-and-rewards/article/decommissioning-of-online-services-for-older-legacy-ubisoft-games-a-m/000064576) for the game's online services, which implies termination of the [EULA](https://www.ubisoft.com/legal/documents/eula/en-US).

All rights belong to their respective owners.
