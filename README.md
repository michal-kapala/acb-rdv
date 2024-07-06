# Assassin's Creed Brotherhood RDV

Matchmaking/online service (Quazal's Rendez-Vous, RDV) backend for AC Brotherhood.

There's no need for dedicated game server implementation as AC multiplayers are all P2P.

Credits for the original implementation of GRO backend go to [@Warranty Voider](https://github.com/zeroKilo).

## Configuration

As of now, you need to create an SQLite database with `users` table:

```sql
CREATE TABLE "users" (
    "id"    INTEGER PRIMARY KEY AUTOINCREMENT,
    "pid"    BIGINT,
    "name"    TEXT,
    "password"    TEXT,
    "ubi_id"    TEXT,
    "email"    TEXT,
    "country_code"    TEXT,
    "pref_lang"    TEXT
)
```

### Users

Add `Tracking` user for the game's telemetry service:

```sql
INSERT INTO users VALUES (1,'Tracking','JaDe!','1234abcd-5678-90ef-4321-0987654321fe','tracking@ubi.com', 'US', 'en')
```

You need your real credentials, email and [Ubi account ID](https://www.reddit.com/r/uplay/comments/piyp3h/how_to_find_your_ubisoft_connect_account_id/) to be able to log in as these are passed from Ubi Connect.

Add your Ubi account credentials to log in with:

```sql
INSERT INTO users VALUES (2,'<Ubi username>','<Ubi password>','<Ubi account ID>', '<Ubi email>', '<country code>', '<preferred language code>')
```

The database file is excluded from git tracking. Once a proper schema emerges it will be committed.

#### How to get the tool to work

##### 1.Build

When you are building the solution, you should not build the whole solution, because it will error out, you only need to build the `ACB RDV` project.

Furthermore, you might need to add references to Be.Windows.Forms.HexBox to the QuazalWV depending on various factors, you can do that by right clicking on the project and clicking on add->reference.

You might also need an app.config file in some folders (the compile errors will specify) which should look something like:

```xml
<?xml version="1.0" encoding="utf-8"?>
<configuration>
    <startup> 
        <supportedRuntime version="v4.0" sku=".NETFramework,Version=v4.8"/>
    </startup>
</configuration>
```



##### 2.Database File

The database file should be in `SQLite` binary format (you can use sqlite dbbrowser to create it). It should also have the field `pid` filled in (with a random integer). It should be placed in the release build folder after building it. (you can take the schema from the top of the readme page). 

![](DocResources\2024-07-06-13-35-39-image.png)

The database should be named `database.sqlite` and placed in the Release folder that contains the tool we built during the previous step.

##### 3.Runtime

You need to add `127.0.0.1 onlineconfigservice.ubi.com` to your hosts file, this is done to redirect the ubisoft server calls to the localhost hosted server.

The next step is making sure you give permissions to the release binary to open the following ports : 

- TCP 80 for online config service (check if you have iis disabled)
- UDP 21030 (quazal authentication)
- UDP 21031 (RDV)

After you make sure, open the binary, start the server and then start the game using the following command:

ACBMP.exe /launchfromotherexec /onlineUser:<ubi user> /onlinePassword:<ubi password>

The `user` and `password` should be the ones from your database file ( the second user that is not the tracking user) 

If everything is successful you will see some messages that look like this upon starting the game and getting to the loading screen:

`7/6/2024 9:54:25 AM : [01][UDP Secure] `*CONNECT*`: PID: 0x00001234, CID: 0, response code 0xE2450886`

If something went wrong, then most messages will be like : 

`7/6/2024 9:54:25 AM : [01][RMC] Received Request : [RMC Packet : Proto = AuthenticationService CallID=28 MethodID=2]`