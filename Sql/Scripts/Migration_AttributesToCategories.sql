-- One-off migration: move Attribute / ProviderAttribute data into
-- Category, SubCategory, ProviderCategory and ProviderSubCategory.
--
-- Run once against a database imported from little-help-book.bacpac,
-- which still has the Attribute and ProviderAttribute tables.
--   * Top-level attributes (ParentAttributeId IS NULL) become categories.
--   * Child attributes become subcategories under their parent.
--   * Category and subcategory IDs keep the original AttributeId.
--   * ProviderCategory gets each provider's direct category links plus
--     the parent category of every subcategory the provider is linked to.

SET NOCOUNT ON;
GO

-- 1. Create the new tables if they don't exist yet.
IF OBJECT_ID(N'dbo.Category', N'U') IS NULL
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

IF OBJECT_ID(N'dbo.SubCategory', N'U') IS NULL
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

IF OBJECT_ID(N'dbo.ProviderCategory', N'U') IS NULL
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

IF OBJECT_ID(N'dbo.ProviderSubCategory', N'U') IS NULL
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

-- 2. Copy the data in one transaction.
SET XACT_ABORT ON;

IF EXISTS (SELECT 1 FROM dbo.Category) OR EXISTS (SELECT 1 FROM dbo.SubCategory)
   OR EXISTS (SELECT 1 FROM dbo.ProviderCategory) OR EXISTS (SELECT 1 FROM dbo.ProviderSubCategory)
    THROW 50000, 'Category tables already contain data; this migration has already run.', 1;

BEGIN TRANSACTION;

-- Categories: top-level attributes, keeping AttributeId.
SET IDENTITY_INSERT dbo.Category ON;
INSERT INTO dbo.Category (CategoryId, Name, Description, l10N, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT AttributeId, Name, Description, l10N, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn
FROM dbo.Attribute
WHERE ParentAttributeId IS NULL;
SET IDENTITY_INSERT dbo.Category OFF;

-- Subcategories: child attributes under their parent, keeping AttributeId.
SET IDENTITY_INSERT dbo.SubCategory ON;
INSERT INTO dbo.SubCategory (SubCategoryId, CategoryId, Name, Description, l10N, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT AttributeId, ParentAttributeId, Name, Description, l10N, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn
FROM dbo.Attribute
WHERE ParentAttributeId IS NOT NULL;
SET IDENTITY_INSERT dbo.SubCategory OFF;

-- Provider subcategory links (one row per provider/subcategory pair).
WITH Links AS (
    SELECT pa.ProviderId, pa.AttributeId AS SubCategoryId,
           pa.CreatedBy, pa.CreatedOn, pa.ModifiedBy, pa.ModifiedOn,
           ROW_NUMBER() OVER (PARTITION BY pa.ProviderId, pa.AttributeId
                              ORDER BY pa.CreatedOn, pa.ProviderAttributeId) AS rn
    FROM dbo.ProviderAttribute pa
    JOIN dbo.SubCategory sc ON sc.SubCategoryId = pa.AttributeId
)
INSERT INTO dbo.ProviderSubCategory (ProviderId, SubCategoryId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT ProviderId, SubCategoryId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn
FROM Links
WHERE rn = 1;

-- Provider category links: direct category links plus the parents of
-- linked subcategories (one row per provider/category pair; a direct
-- link's audit values win over a derived one).
WITH Candidates AS (
    SELECT pa.ProviderId, pa.AttributeId AS CategoryId, 0 AS IsDerived,
           pa.CreatedBy, pa.CreatedOn, pa.ModifiedBy, pa.ModifiedOn, pa.ProviderAttributeId
    FROM dbo.ProviderAttribute pa
    JOIN dbo.Category c ON c.CategoryId = pa.AttributeId
    UNION ALL
    SELECT pa.ProviderId, sc.CategoryId, 1 AS IsDerived,
           pa.CreatedBy, pa.CreatedOn, pa.ModifiedBy, pa.ModifiedOn, pa.ProviderAttributeId
    FROM dbo.ProviderAttribute pa
    JOIN dbo.SubCategory sc ON sc.SubCategoryId = pa.AttributeId
),
Ranked AS (
    SELECT *, ROW_NUMBER() OVER (PARTITION BY ProviderId, CategoryId
                                 ORDER BY IsDerived, CreatedOn, ProviderAttributeId) AS rn
    FROM Candidates
)
INSERT INTO dbo.ProviderCategory (ProviderId, CategoryId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn)
SELECT ProviderId, CategoryId, CreatedBy, CreatedOn, ModifiedBy, ModifiedOn
FROM Ranked
WHERE rn = 1;

COMMIT TRANSACTION;
GO

-- 3. Row counts.
SELECT 'Category' AS TableName, COUNT(*) AS Rows FROM dbo.Category
UNION ALL SELECT 'SubCategory', COUNT(*) FROM dbo.SubCategory
UNION ALL SELECT 'ProviderCategory', COUNT(*) FROM dbo.ProviderCategory
UNION ALL SELECT 'ProviderSubCategory', COUNT(*) FROM dbo.ProviderSubCategory;
GO

-- 4. Once the results are checked, drop the old schema deliberately:
-- DROP VIEW IF EXISTS dbo.ProviderWithCatsView;
-- DROP VIEW IF EXISTS dbo.ProviderCategoryView;
-- DROP VIEW IF EXISTS dbo.SubCategoryView;
-- DROP VIEW IF EXISTS dbo.CategoryView;
-- DROP TABLE IF EXISTS dbo.ProviderAttribute;
-- DROP TABLE IF EXISTS dbo.Attribute;
