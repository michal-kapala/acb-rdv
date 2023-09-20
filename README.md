# Assassin's Creed Brotherhood RDV

Matchmaking/online service (Quazal's Rendez-Vous, RDV) backend for AC Brotherhood.

There's no need for dedicated game server implementation as AC multiplayers are all P2P.

Credits for the original implementation of GRO backend go to [@Warranty Voider](https://github.com/zeroKilo).

## Configuration

As of now, you need to get to create SQLite database with `users` table:
```sql
CREATE TABLE "users" (
	"id"	INTEGER PRIMARY KEY AUTOINCREMENT,
	"pid"	BIGINT,
	"name"	TEXT,
	"password"	TEXT
)
```

### Users

Add `Tracking` user for the game's telemetry service:
```sql
INSERT INTO users VALUES (1,'Tracking','JaDe!')
```

Add your Ubi account credentials to log in with:
```sql
INSERT INTO users VALUES (2,'<Ubi username>','<Ubi password>')
```

The database file is excluded from git tracking. Once a proper schema emerges it will be committed.