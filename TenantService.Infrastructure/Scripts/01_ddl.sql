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
    username character varying(32) COLLATE pg_catalog."default" NOT NULL,
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
    user_id uuid NOT NULL,
    tenant_id character varying(10) COLLATE pg_catalog."default" NOT NULL,
    write boolean NOT NULL,
    CONSTRAINT acl_pkey PRIMARY KEY (user_id, tenant_id)
) TABLESPACE pg_default;

ALTER TABLE IF EXISTS infra.acl OWNER to fiorencis;    


-- Table: application.tenant - tenants' information
CREATE TABLE IF NOT EXISTS application.tenant
(
    id character varying(10) COLLATE pg_catalog."default" NOT NULL,
    name character varying(256) COLLATE pg_catalog."default" NOT NULL,
    tax_code character varying(16) COLLATE pg_catalog."default" NOT NULL,
    subscription_date date NOT NULL DEFAULT CURRENT_DATE,
    subscriber_email character varying(64) COLLATE pg_catalog."default" NOT NULL,
    license_id uuid NOT NULL,
    disposal_date date,
    status smallint NOT NULL,
    CONSTRAINT tenant_pkey PRIMARY KEY (id)
) TABLESPACE pg_default;    

ALTER TABLE IF EXISTS application.tenant OWNER to fiorencis; 

CREATE TABLE IF NOT EXISTS application.license
(
    id uuid NOT NULL,
    serialNumber character varying(32) COLLATE pg_catalog."default" NOT NULL,
    name character varying(64) COLLATE pg_catalog."default" NOT NULL,
    max_users integer NOT NULL,
    CONSTRAINT tenant_license_pkey PRIMARY KEY (id)
) TABLESPACE pg_default; 

ALTER TABLE IF EXISTS application.license OWNER to fiorencis;


ALTER TABLE IF EXISTS infra.user
    ADD CONSTRAINT user_ukey_username UNIQUE (username);


-- foreign keys
ALTER TABLE IF EXISTS infra.acl
    ADD CONSTRAINT acl_fkey_tenant FOREIGN KEY (tenant_id)
    REFERENCES application.tenant (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;


ALTER TABLE IF EXISTS infra.acl
    ADD CONSTRAINT acl_fkey_user FOREIGN KEY (user_id)
    REFERENCES infra.user (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION
    NOT VALID;


ALTER TABLE IF EXISTS application.tenant
    ADD CONSTRAINT tenant_fkey_license FOREIGN KEY (license_id)
    REFERENCES application.license (id) MATCH SIMPLE
    ON UPDATE NO ACTION
    ON DELETE NO ACTION 
    NOT VALID;    


CREATE TABLE IF NOT EXISTS infra.refreshtoken 
(
    id bigint GENERATED ALWAYS AS IDENTITY,
    token character varying(256) COLLATE pg_catalog."default" NOT NULL,
    username character varying(32) COLLATE pg_catalog."default" NOT NULL,
    expiresat TIMESTAMPTZ NOT NULL,
    createdat TIMESTAMPTZ NOT NULL,
    isrevoked boolean NOT NULL,
    CONSTRAINT refresh_token_pkey PRIMARY KEY (id)

)