-- Script
INSERT INTO [SsoUser] ([Id], [IdApplicationUser], [RazonSocial], [Cuit], [TipoUsuario], [Email], [IsForeignCuit]) VALUES (1, 'e644b27e-bfa6-44df-d89a-08d7d7e53939', 'TEST_1', '20183853792', 'Usuario', 'millanestest@gmail.com',false);
INSERT INTO [SsoUser] ([Id], [IdApplicationUser], [RazonSocial], [Cuit], [TipoUsuario], [Email], [IsForeignCuit]) VALUES (2, '1faa4d5e-98a4-48bb-899b-08d82feb7fec', 'TEST_2', '20236698824', 'Usuario', 'millanestest@gmail.com',false);
INSERT INTO [SsoUser] ([Id], [IdApplicationUser], [RazonSocial], [Cuit], [TipoUsuario], [Email], [IsForeignCuit]) VALUES (3, '2c335a97-a666-473f-8996-08d82feb7fec', 'TEST_3', '27927525408', 'Usuario', 'millanestest@gmail.com',false);
INSERT INTO [SsoUser] ([Id], [IdApplicationUser], [RazonSocial], [Cuit], [TipoUsuario], [Email], [IsForeignCuit]) VALUES (4, '6a427f2e-2ad4-497a-fbc9-08d7a10d5328', 'TEST_4', '20312023154', 'Administrador', 'millanestest@gmail.com',false);
INSERT INTO [SsoUser] ([Id], [IdApplicationUser], [RazonSocial], [Cuit], [TipoUsuario], [Email], [IsForeignCuit]) VALUES (5, 'eb350997-8f96-47a2-8af9-08d7a10f44ce', 'TEST_5', '20321120569', 'Cliente', 'nextComTest@gmail.com',false);

INSERT INTO [SsoUserRoles] ([Id], [UserId], [Role]) VALUES (1, 'e644b27e-bfa6-44df-d89a-08d7d7e53939', 'Cliente');
INSERT INTO [SsoUserRoles] ([Id], [UserId], [Role]) VALUES (2, '1faa4d5e-98a4-48bb-899b-08d82feb7fec', 'Cliente');
INSERT INTO [SsoUserRoles] ([Id], [UserId], [Role]) VALUES (3, '2c335a97-a666-473f-8996-08d82feb7fec', 'Cliente');
INSERT INTO [SsoUserRoles] ([Id], [UserId], [Role]) VALUES (4, '6a427f2e-2ad4-497a-fbc9-08d7a10d5328', 'Admin');
INSERT INTO [SsoUserRoles] ([Id], [UserId], [Role]) VALUES (5, '6a427f2e-2ad4-497a-fbc9-08d7a10d5328', 'CuentasACobrar');