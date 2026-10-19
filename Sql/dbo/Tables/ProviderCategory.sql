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
