UPDATE AdvanceFees
SET Status = 1
WHERE Status = 0

INSERT INTO [dbo].[PermissionsXRoles] (Code, RoleName)
VALUES (22, 'Admin');
INSERT INTO [dbo].[PermissionsXRoles] (Code, RoleName)
VALUES (22, 'CuentasACobrar');