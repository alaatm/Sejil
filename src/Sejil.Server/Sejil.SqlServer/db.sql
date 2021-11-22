-- Copyright (C) 2021 Alaa Masoud
-- See the LICENSE file in the project root for more information.

IF NOT EXISTS (SELECT *
FROM INFORMATION_SCHEMA.SCHEMATA
WHERE SCHEMA_NAME='sejil')
	EXEC('CREATE SCHEMA [sejil] AUTHORIZATION [dbo]');

IF NOT EXISTS (SELECT *
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='sejil' AND TABLE_NAME='log')
	CREATE TABLE [sejil].[log]
(
    [id] uniqueidentifier NOT NULL PRIMARY KEY,
    [message] NVARCHAR(MAX) NOT NULL,
    [messageTemplate] NVARCHAR(MAX) NOT NULL,
    [level] VARCHAR(64) NOT NULL,
    [timestamp] DATETIME2 NOT NULL,
    [exception] NVARCHAR(MAX) NULL,

    INDEX IX_log_level_idx NONCLUSTERED ([level]),
);

IF NOT EXISTS (SELECT *
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='sejil' AND TABLE_NAME='log_property')
	CREATE TABLE [sejil].[log_property]
(
    [id] INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [logId] uniqueidentifier NOT NULL,
    [name] NVARCHAR(MAX) NOT NULL,
    [value] NVARCHAR(MAX) NULL,
    FOREIGN KEY([logId]) REFERENCES [sejil].[log]([id])
);

IF NOT EXISTS (SELECT *
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA='sejil' AND TABLE_NAME='log_query')
	CREATE TABLE [sejil].[log_query]
(
    [id] INTEGER NOT NULL IDENTITY(1,1) PRIMARY KEY,
    [name] NVARCHAR(MAX) NOT NULL,
    [query] NVARCHAR(MAX) NOT NULL,
);
