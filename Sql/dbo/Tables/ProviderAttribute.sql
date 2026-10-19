CREATE TABLE [dbo].[ProviderAttribute] (
    [ProviderAttributeId] INT            IDENTITY (1, 1) NOT NULL,
    [ProviderId]          INT            NOT NULL,
    [AttributeId]         INT            NOT NULL,
    [CreatedBy]           NVARCHAR (256) NOT NULL,
    [CreatedOn]           DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    [ModifiedBy]          NVARCHAR (256) NOT NULL,
    [ModifiedOn]          DATETIME2 (7)  DEFAULT (getdate()) NOT NULL,
    CONSTRAINT [PK_ProviderAttribute] PRIMARY KEY CLUSTERED ([ProviderAttributeId] ASC)
);

