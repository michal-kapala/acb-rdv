# Assassin's Creed Brotherhood RDV

Matchmaking/online service (Quazal's Rendez-Vous, RDV) backend for AC Brotherhood.

There's no need for dedicated game server implementation as AC multiplayers are all P2P.

Credits for the original implementation of GRO backend go to [@Warranty Voider](https://github.com/zeroKilo).

## Configuration

As of now, you need to create an SQLite database with `users` table:
```sql
CREATE TABLE "users" (
	"id"	INTEGER PRIMARY KEY AUTOINCREMENT,
	"pid"	BIGINT,
	"name"	TEXT,
	"password"	TEXT,
	"ubi_id"	TEXT,
	"email"	TEXT,
	"country_code"	TEXT,
	"pref_lang"	TEXT
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