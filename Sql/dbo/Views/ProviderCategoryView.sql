
CREATE VIEW [dbo].[ProviderCategoryView]
AS
SELECT p.ProviderId, ac.Name AS category
FROM     dbo.Provider AS p LEFT OUTER JOIN
        dbo.ProviderAttribute AS pa ON p.ProviderId = pa.ProviderId LEFT OUTER JOIN
            (SELECT AttributeId, Name
            FROM      dbo.Attribute
            WHERE   (ParentAttributeId IS NULL)) AS ac ON pa.AttributeId = ac.AttributeId