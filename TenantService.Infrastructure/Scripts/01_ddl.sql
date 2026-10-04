/* DDL script to create the database schemas, and tables for managing 
   tenant information and user accounts.
 
   This script sets up the foundational database structure for the tenant service from scratch.
   ==============================================================================================
   Version: 1.0.717 
*/

-- Creates the infrastructure schema for the tenant service, which contains tables 
-- and other database objects related to the service's internal operations and management.
CREATE SCHEMA infra
    AUTHORIZATION fiorencis;

CREATE SCHEMA application
    AUTHORIZATION fiorencis;


-- infra schema tables
-- database update tracking table to keep track of applied database migrations
CREATE TABLE IF NOT EXISTS infra.dbupdate
(
    id integer NOT NULL GENERATED ALWAYS AS IDENTITY ( INCREMENT 1 START 1 MINVALUE 1 MAXVALUE 2147483647 CACHE 1 ),
    version character varying(32) COLLATE pg_catalog."default" NOT NULL,
    appliedat TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT dbupdate_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;

ALTER TABLE IF EXISTS infra.dbupdate OWNER to fiorencis;

-- Table: infra.user - administrator's login accounts to manipulate tenants
CREATE TABLE IF NOT EXISTS infra.user
(
    id uuid NOT NULL,
    username character varying(64) COLLATE pg_catalog."default" NOT NULL,
    fullName character varying(256) COLLATE pg_catalog."default" NOT NULL,
    email character varying(256) COLLATE pg_catalog."default" NOT NULL,
    passwordHash character varying(128) COLLATE pg_catalog."default",
    admin boolean NOT NULL DEFAULT false,
    status smallint NOT NULL DEFAULT 0,
    CONSTRAINT user_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;

ALTER TABLE IF EXISTS infra.user OWNER to fiorencis;

CREATE TABLE IF NOT EXISTS infra.acl
(
    userId uuid NOT NULL,
    tenantId uuid NOT NULL,
    write boolean NOT NULL,
    CONSTRAINT acl_pkey PRIMARY KEY (userId, tenantId)
) TABLESPACE pg_default;

ALTER TABLE IF EXISTS infra.acl OWNER to fiorencis;    

CREATE TABLE IF NOT EXISTS infra.refreshtoken 
(
    id bigint GENERATED ALWAYS AS IDENTITY,
    token character varying(256) COLLATE pg_catalog."default" NOT NULL,
    username character varying(32) COLLATE pg_catalog."default" NOT NULL,
    expiresat TIMESTAMPTZ NOT NULL,
    createdat TIMESTAMPTZ NOT NULL,
    isrevoked boolean NOT NULL,
    CONSTRAINT refresh_token_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;

ALTER TABLE IF EXISTS infra.refreshtoken OWNER to fiorencis;

-- application schema tables
-- Table: application.tenant - tenants' information
CREATE TABLE IF NOT EXISTS application.tenant
(
    id uuid NOT NULL,
    code character varying(24) COLLATE pg_catalog."default" NOT NULL,
    name character varying(256) COLLATE pg_catalog."default" NOT NULL,
    taxCode character varying(16) COLLATE pg_catalog."default" NOT NULL,
    email character varying(64) COLLATE pg_catalog."default" NOT NULL,
    subscriptionDate date NOT NULL DEFAULT CURRENT_DATE,
    disposalDate date,
    status smallint NOT NULL,
    notes text NULL,
    CONSTRAINT tenant_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;    

ALTER TABLE IF EXISTS application.tenant OWNER to fiorencis; 

-- license table to store license information for tenants
CREATE TABLE IF NOT EXISTS application.license
(
    id uuid NOT NULL,
    tenantid uuid NOT NULL,
    serialNumber character varying(32) COLLATE pg_catalog."default" NOT NULL,
    name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    maxusers integer NOT NULL,
    CONSTRAINT tenant_license_pkey PRIMARY KEY (id)
) TABLESPACE pg_default; 

ALTER TABLE IF EXISTS application.license OWNER to fiorencis;

-- constraints and indexes
-- username unique constraint
ALTER TABLE IF EXISTS infra.user
    ADD CONSTRAINT user_ukey_username UNIQUE (username);

ALTER TABLE IF EXISTS application.tenant
    ADD CONSTRAINT tenant_ukey_code UNIQUE (code);

-- foreign keys
-- tenant foreign key constraints for ACL table
ALTER TABLE IF EXISTS infra.acl
    ADD CONSTRAINT acl_fkey_tenant FOREIGN KEY (tenantid)
    REFERENCES application.tenant (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

-- user foreign key constraints for ACL table
ALTER TABLE IF EXISTS infra.acl
    ADD CONSTRAINT acl_fkey_user FOREIGN KEY (userid)
    REFERENCES infra.user (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;


-- license foreign key constraints for tenant table
ALTER TABLE IF EXISTS application.license
    ADD CONSTRAINT license_fkey_tenant FOREIGN KEY (tenantid)
    REFERENCES application.tenant (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;

