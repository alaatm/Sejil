CREATE TABLE IF NOT EXISTS log(
	id TEXT PRIMARY KEY,
	message TEXT NOT NULL,
	message_template TEXT NOT NULL,
	level VARCHAR(64) NOT NULL,
	timestamp DATETIME NOT NULL,
	exception TEXT NULL
);

CREATE INDEX IF NOT EXISTS log_message_idx ON log(message);

CREATE INDEX IF NOT EXISTS log_level_idx ON log(level);

CREATE INDEX IF NOT EXISTS log_timestamp_idx ON log(timestamp);

CREATE INDEX IF NOT EXISTS log_exception_idx ON log(exception);

CREATE TABLE IF NOT EXISTS log_property(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	log_id TEXT NOT NULL,
	name TEXT NOT NULL,
	value TEXT NULL,
	FOREIGN KEY(log_id) REFERENCES log(id)
);

CREATE INDEX IF NOT EXISTS log_property_log_id_idx ON log_property(log_id);

CREATE INDEX IF NOT EXISTS log_property_name_idx ON log_property(name);

CREATE INDEX IF NOT EXISTS log_property_value_idx ON log_property(value);

CREATE TABLE IF NOT EXISTS log_query(
	id INTEGER PRIMARY KEY AUTOINCREMENT,
	name VARCHAR(255) NOT NULL,
	query TEXT NOT NULL
);