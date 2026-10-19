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
