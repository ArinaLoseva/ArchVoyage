﻿CREATE TABLE IF NOT EXISTS "__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL,
    CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId")
);

START TRANSACTION;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250429172800_InitialCreate') THEN
    CREATE TABLE "TechData" (
        "UserId" integer GENERATED BY DEFAULT AS IDENTITY,
        "RefreshToken" text,
        "RefreshTokenExpiryTime" timestamp with time zone,
        CONSTRAINT "PK_TechData" PRIMARY KEY ("UserId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250429172800_InitialCreate') THEN
    CREATE TABLE "Users" (
        "UserId" integer GENERATED BY DEFAULT AS IDENTITY,
        "Email" text NOT NULL,
        "PasswordHash" text,
        CONSTRAINT "PK_Users" PRIMARY KEY ("UserId")
    );
    END IF;
END $EF$;

DO $EF$
BEGIN
    IF NOT EXISTS(SELECT 1 FROM "__EFMigrationsHistory" WHERE "MigrationId" = '20250429172800_InitialCreate') THEN
    INSERT INTO "__EFMigrationsHistory" ("MigrationId", "ProductVersion")
    VALUES ('20250429172800_InitialCreate', '9.0.4');
    END IF;
END $EF$;
COMMIT;

