CREATE VIEW [dbo].[SubCategoryView]
AS
select AttributeId,ParentAttributeId as CategoryId, Name from dbo.Attribute where ParentAttributeId is not null