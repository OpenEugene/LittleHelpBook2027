-- Creates the Little Help Book app tables. Run before Load_AppData.sql.
-- Drops the app tables first if they exist, plus the old Attribute schema
-- left by earlier attempts. Oqtane's own tables are not touched.
SET NOCOUNT ON;
GO

-- Old attribute-based schema
DROP VIEW IF EXISTS dbo.ProviderWithCatsView;
DROP VIEW IF EXISTS dbo.ProviderCategoryView;
DROP VIEW IF EXISTS dbo.SubCategoryView;
DROP VIEW IF EXISTS dbo.CategoryView;
DROP TABLE IF EXISTS dbo.ProviderAttribute;
DROP TABLE IF EXISTS dbo.Attribute;

-- App tables (no foreign keys, so order doesn't matter)
DROP TABLE IF EXISTS dbo.[ProviderSubCategory];
DROP TABLE IF EXISTS dbo.[ProviderCategory];
DROP TABLE IF EXISTS dbo.[PhoneNumber];
DROP TABLE IF EXISTS dbo.[Address];
DROP TABLE IF EXISTS dbo.[Provider];
DROP TABLE IF EXISTS dbo.[SubCategory];
DROP TABLE IF EXISTS dbo.[Category];
GO

CREATE TABLE [dbo].[Category] (
    [CategoryId]  INT             IDENTITY (1, 1) NOT NULL,
    [Name]        NVARCHAR (200)  NOT NULL,
    [Description] NVARCHAR (500)  NULL,
    [l10N]        NVARCHAR (4000) NULL,
    [CreatedBy]   NVARCHAR (256)  NOT NULL,
    [CreatedOn]   DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]  NVARCHAR (256)  NOT NULL,
    [ModifiedOn]  DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Category] PRIMARY KEY CLUSTERED ([CategoryId] ASC)
);
GO

CREATE TABLE [dbo].[SubCategory] (
    [SubCategoryId] INT             IDENTITY (1, 1) NOT NULL,
    [CategoryId]    INT             NOT NULL,
    [Name]          NVARCHAR (200)  NOT NULL,
    [Description]   NVARCHAR (500)  NULL,
    [l10N]          NVARCHAR (4000) NULL,
    [CreatedBy]     NVARCHAR (256)  NOT NULL,
    [CreatedOn]     DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]    NVARCHAR (256)  NOT NULL,
    [ModifiedOn]    DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_SubCategory] PRIMARY KEY CLUSTERED ([SubCategoryId] ASC)
);
GO

CREATE TABLE [dbo].[Provider] (
    [ProviderId]       INT             IDENTITY (1, 1) NOT NULL,
    [Name]             NVARCHAR (200)  NOT NULL,
    [Description]      NVARCHAR (2000) NULL,
    [WebAddress]       NVARCHAR (500)  NULL,
    [EmailAddress]     NVARCHAR (100)  NULL,
    [HoursOfOperation] NVARCHAR (500)  NULL,
    [IsActive]         BIT             DEFAULT ((1)) NOT NULL,
    [l10N]             NVARCHAR (MAX)  NULL,
    [CreatedBy]        NVARCHAR (256)  NOT NULL,
    [CreatedOn]        DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]       NVARCHAR (256)  NOT NULL,
    [ModifiedOn]       DATETIME2 (7)   DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Provider] PRIMARY KEY CLUSTERED ([ProviderId] ASC)
);
GO

CREATE TABLE [dbo].[Address] (
    [AddressId]           INT            IDENTITY (1, 1) NOT NULL,
    [ProviderId]          INT            NULL,
    [Address1]            NVARCHAR (120) NULL,
    [Address2]            NVARCHAR (120) NULL,
    [Address3]            NVARCHAR (120) NULL,
    [City]                NVARCHAR (100) NULL,
    [State]               CHAR (2)       NULL,
    [PostalCode]          VARCHAR (16)   NULL,
    [HasWheelchairAccess] BIT            CONSTRAINT [DF_Address_HasWheelchairAccess] DEFAULT ((0)) NOT NULL,
    [HasLanguageSupport]  BIT            CONSTRAINT [DF_Address_HasLanguageSupport] DEFAULT ((0)) NOT NULL,
    [Geocoding]           VARCHAR (64)   NULL,
    [Longitude]           REAL           NULL,
    [Latitude]            REAL           NULL,
    [IsActive]            BIT            CONSTRAINT [DF__Address__IsActiv__2DE6D218] DEFAULT ((1)) NOT NULL,
    [CreatedBy]           NVARCHAR (256) NOT NULL,
    [CreatedOn]           DATETIME2 (7)  CONSTRAINT [DF__Address__Created__2CF2ADDF] DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]          NVARCHAR (256) NOT NULL,
    [ModifiedOn]          DATETIME2 (7)  CONSTRAINT [DF__Address__Modifie__2BFE89A6] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_Address] PRIMARY KEY CLUSTERED ([AddressId] ASC)
);
GO

CREATE TABLE [dbo].[PhoneNumber] (
    [PhoneNumberId] INT             IDENTITY (1, 1) NOT NULL,
    [ProviderId]    INT             NULL,
    [CountryCode]   INT             NULL,
    [AreaCode]      INT             NULL,
    [Number]        NVARCHAR (500)  NULL,
    [Extension]     INT             NULL,
    [Description]   NVARCHAR (500)  NULL,
    [l10N]          NVARCHAR (4000) NULL,
    [IsActive]      BIT             CONSTRAINT [DF_PhoneNumber_IsActive] DEFAULT ((1)) NULL,
    [CreatedBy]     NVARCHAR (256)  NOT NULL,
    [CreatedOn]     DATETIME2 (7)   CONSTRAINT [DF__PhoneNumb__Creat__1F98B2C1] DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]    NVARCHAR (256)  NOT NULL,
    [ModifiedOn]    DATETIME2 (7)   CONSTRAINT [DF__PhoneNumb__Modif__208CD6FA] DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_PhoneNumber] PRIMARY KEY CLUSTERED ([PhoneNumberId] ASC)
);
GO

CREATE TABLE [dbo].[ProviderCategory] (
    [ProviderCategoryId] INT            IDENTITY (1, 1) NOT NULL,
    [ProviderId]         INT            NOT NULL,
    [CategoryId]         INT            NOT NULL,
    [CreatedBy]          NVARCHAR (256) NOT NULL,
    [CreatedOn]          DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]         NVARCHAR (256) NOT NULL,
    [ModifiedOn]         DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProviderCategory] PRIMARY KEY CLUSTERED ([ProviderCategoryId] ASC),
    CONSTRAINT [UQ_ProviderCategory_ProviderId_CategoryId] UNIQUE ([ProviderId], [CategoryId])
);
GO

CREATE TABLE [dbo].[ProviderSubCategory] (
    [ProviderSubCategoryId] INT            IDENTITY (1, 1) NOT NULL,
    [ProviderId]            INT            NOT NULL,
    [SubCategoryId]         INT            NOT NULL,
    [CreatedBy]             NVARCHAR (256) NOT NULL,
    [CreatedOn]             DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]            NVARCHAR (256) NOT NULL,
    [ModifiedOn]            DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProviderSubCategory] PRIMARY KEY CLUSTERED ([ProviderSubCategoryId] ASC),
    CONSTRAINT [UQ_ProviderSubCategory_ProviderId_SubCategoryId] UNIQUE ([ProviderId], [SubCategoryId])
);
GO
