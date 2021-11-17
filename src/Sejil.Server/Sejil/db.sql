-- Copyright (C) 2017 Alaa Masoud
-- See the LICENSE file in the project root for more information.

CREATE TABLE IF NOT EXISTS log(
	id              TEXT        NOT NULL    PRIMARY KEY,
	message         TEXT        NOT NULL    COLLATE NOCASE,
	messageTemplate TEXT        NOT NULL,
	level           VARCHAR(64) NOT NULL,
	timestamp       DATETIME    NOT NULL,
	exception       TEXT        NULL        COLLATE NOCASE
);

CREATE INDEX IF NOT EXISTS log_message_idx		ON log(message);

CREATE INDEX IF NOT EXISTS log_level_idx		ON log(level);

CREATE INDEX IF NOT EXISTS log_timestamp_idx	ON log(timestamp);

CREATE INDEX IF NOT EXISTS log_exception_idx	ON log(exception);

CREATE TABLE IF NOT EXISTS log_property(
	id      INTEGER NOT NULL    PRIMARY KEY AUTOINCREMENT,
	logId   TEXT    NOT NULL,
	name    TEXT    NOT NULL    COLLATE NOCASE,
	value   TEXT    NULL        COLLATE NOCASE,
	FOREIGN KEY(logId) REFERENCES log(id)
);

CREATE INDEX IF NOT EXISTS log_property_logId_idx	ON log_property(logId);

CREATE INDEX IF NOT EXISTS log_property_name_idx	ON log_property(name);

CREATE INDEX IF NOT EXISTS log_property_value_idx	ON log_property(value);

CREATE TABLE IF NOT EXISTS log_query(
	id      INTEGER      NOT NULL PRIMARY KEY AUTOINCREMENT,
	name    VARCHAR(255) NOT NULL,
	query   TEXT         NOT NULL
);

CREATE INDEX IF NOT EXISTS log_query_name_idx ON log_query(name);
