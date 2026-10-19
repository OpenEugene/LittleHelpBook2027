CREATE VIEW [dbo].[CategoryView]
AS
select AttributeId, Name from dbo.Attribute where ParentAttributeId is null