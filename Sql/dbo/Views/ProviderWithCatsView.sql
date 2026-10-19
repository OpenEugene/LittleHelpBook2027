CREATE VIEW [dbo].[ProviderWithCatsView]
AS
SELECT 
    p.ProviderId,
    p.Name,
    p.Description,
    p.WebAddress,
    p.EmailAddress,
    p.HoursOfOperation,
    STRING_AGG(ac.Name, ', ')  as Categories,
    STRING_AGG(sc.Name, ', ')  as Subcategories
FROM     
    dbo.Provider AS p 
    LEFT OUTER JOIN dbo.ProviderAttribute pa ON p.ProviderId = pa.ProviderId
    LEFT OUTER JOIN (select AttributeId, Name from dbo.Attribute where ParentAttributeId is null) ac ON pa.AttributeId = ac.AttributeId
    LEFT OUTER JOIN (select AttributeId, Name from dbo.Attribute where ParentAttributeId is not null) sc ON pa.AttributeId = sc.AttributeId
GROUP BY 
    p.ProviderId,
    p.Name,
    p.Description,
    p.WebAddress,
    p.EmailAddress,
    p.HoursOfOperation