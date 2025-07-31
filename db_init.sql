CREATE TABLE IF NOT EXISTS "users" (
    "id"            INTEGER PRIMARY KEY AUTOINCREMENT,
    "pid"           BIGINT,
    "name"          TEXT,
    "password"      TEXT,
    "ubi_id"        TEXT,
    "email"         TEXT,
    "country_code"  TEXT,
    "pref_lang"     TEXT
);

INSERT INTO users (pid, name, password, ubi_id, email, country_code, pref_lang)
VALUES
(105,'Tracking','JaDe!','1234abcd-5678-90ef-4321-0987654321fe','tracking@ubi.com', 'US', 'en'),
(1001,'Player','pass','1234abcd-5678-90ef-4321-0987654321fe','player@notubi.com', 'US', 'en');

CREATE TABLE IF NOT EXISTS "privileges" (
    id          INTEGER,
    description TEXT,
    locale      TEXT
);

INSERT INTO privileges VALUES
(1, 'Allow to play online', 'en-US'),
(1000, 'Trajan Market Map', 'en-US'),
(1001, 'Aqueduct Map', 'en-US'),
(1004, 'Ezio''s Helmschmied Drachen Armor Skin', 'en-US'),
(1005, 'Harlequin', 'en-US'),
(1006, 'Officer', 'en-US');

CREATE TABLE IF NOT EXISTS "relationships" (
    requester INTEGER NOT NULL,
    requestee INTEGER NOT NULL,
    type      TINYINT NOT NULL,
    details   INTEGER,

    PRIMARY KEY (requester, requestee)
);

CREATE TABLE IF NOT EXISTS "messages" (
    id              INTEGER,
    recipient_pid   INTEGER NOT NULL,
    recipient_type  INTEGER,
    parent_id       INTEGER,
    sender_pid      INTEGER,
    reception_time  INTEGER,
    lifetime        INTEGER,
    flags           INTEGER,
    subject         TEXT,
    sender_name     TEXT,
    body            TEXT,
    delivered       INTEGER,
	
    PRIMARY KEY(id)
);

CREATE TABLE IF NOT EXISTS "rewards" (
	code        TEXT,
	name        TEXT,
	description TEXT,
	val         INTEGER
);

INSERT INTO rewards VALUES
('ACBREWARD02', 'Ubiór florenckiego szlachcica', 'Ubierz Ezio w strój, który nosił, gdy był florenckim szlachcicem.', 20),
('ACBREWARD03', 'Zbroja Altaïra', 'Ubierz Ezio w zbroję Altaïra.', 20),
('ACBREWARD04', 'Szaty Altaïra', 'Ubierz Ezio w asasyńskie szaty Altaïra.', 20),
('ACBREWARD05', 'Ulepszenie zasobnika broni', 'Zwiększ pojemność broni palnej do 10 naboi.', 30),
('ACBREWARD06', 'Odblokuj Hellequin', 'Graj jako Hellequin.', 40),
('ACBREWARD07', 'Porwanie Da Vinciego', 'Pomóż Ezio odbić porwanego Leonarda i poznaj nowe możliwości, jakie oferuje ten największy dodatek dla pojedynczego gracza.', 0);

CREATE TABLE IF NOT EXISTS "tags" (
    tag TEXT UNIQUE NOT NULL
);

INSERT INTO tags VALUES
('LINKAPP_VIEW'),
('UPLAY_START'),
('UPLAY_STOP'),
('GAME_START'),
('GAME_STOP'),
('FPSCLIENT_START'),
('FPSCLIENT_STOP'),
('LEVEL_START'),
('LEVEL_STOP'),
('OBJECTIVE_START'),
('OBJECTIVE_STOP'),
('UPLAY_BROWSE'),
('FPSCLIENT_ABORT'),
('MATCHMAKING_STATS'),
('SEQUENCE_STOP'),
('PLAYER_STATS'),
('AWARD_UNLOCK'),
('PLAYER_DEATH'),
('GAME_SAVE'),
('INSTALL_START'),
('INSTALL_STOP'),
('MENU_ENTER'),
('MENU_EXIT'),
('MENU_OPTIONCHANGE'),
('MM_RES'),
('PLAYER_KILL'),
('PLAYER_SAVED'),
('UNINSTALL_START'),
('UNINSTALL_STOP'),
('VIDEO_START'),
('VIDEO_STOP');

CREATE TABLE IF NOT EXISTS "telemetry_tags" (
	id          INTEGER PRIMARY KEY,
	tracking_id INTEGER NOT NULL,
	tag         TEXT NOT NULL,
	attr        TEXT NOT NULL,
	dtime       INTEGER NOT NULL
);

CREATE TABLE IF NOT EXISTS "game_invites" (
	"id"            INTEGER PRIMARY KEY,
	"inviter"       INTEGER,
	"invitee"       INTEGER,
	"session_type"  INTEGER,
	"session_id"    INTEGER,
	"message"       TEXT
);
