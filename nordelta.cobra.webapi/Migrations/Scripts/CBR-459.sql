-- INSERT PermissionsXRoles
INSERT INTO [dbo].[PermissionsXRoles] (Code, RoleName)
VALUES (24, 'CuentasACobrar');

INSERT INTO [dbo].[PermissionsXRoles] (Code, RoleName)
VALUES (24, 'Admin');

-- INSERT Template
INSERT INTO [dbo].[Template] ([Subject], [Description], [HtmlBody], [Disabled])
VALUES ('Alerta usuario libre de deudas', 'FreeDebtUserTemplate', '<!DOCTYPE html>
<html>
<head>
    <meta charset="utf-8" />
    <title></title>
    <style>
        @font-face {
            font-family: "Graphik";
            src: local("Graphik"), url("{{tipografia}}") format("truetype");
        }
        * {
            font-family: "Graphik", sans-serif;
        }
        .body {
            font-size: 1em;
            color: rgb(0, 47, 85);
        }
        h1 {
            color: rgb(0, 47, 85);
        }
        .footer {
            font-size: 0.8em;
            color: rgb(187, 187, 187);
        }
        p {
            word-break: break-word;
            display: block;
            margin-block-start: 1em;
            margin-block-end: 1em;
            margin-inline-start: 0px;
            margin-inline-end: 0px;
        }
        a {
            color: #598fde;
        }
        .copyright {
            text-align: center;
            color: rgb(0, 47, 85);
        }
        .logo-consultatio {
            padding: 20px 10px;
            display: block;
            margin-left: auto;
            margin-right: auto;
            width: 15em;
        }
        hr {
            border-bottom-color: rgb(232, 232, 232) !important;
        }
    </style>
</head>
<body style="background-color: rgb(246, 246, 246); padding: 20px;">
    <div style="max-width: 560px; margin: 0 auto; background-color: white; padding: 20px 40px;">
        <img class="logo-consultatio" alt="logo_consultatio" src="{{logo_consultatio}}" />
        <hr />
        <h1>
            ¡Hola, <br /> <span style="color: #598fde;">{{CLIENTE_NOMBRE}}!</span>
        </h1>
         <h3><span style="background-color: transparent;">Nos alegra informarte que has finalizado tu plan de pagos para el producto {{PRODUCTO}}.<br><br>
        En breve nuestro equipo de legales se contactará<br>
        para informarte los pasos a seguir
    </h3>
    <br>
    <h3 > Saluda atentamente,<br />Equipo de Cobranza</h3>
        <hr />
        <p class="footer">
            No respondas a este correo porque es automático.
            Este correo electrónico se envió a {{email}}.
            Nota: Este mensaje es confidencial y para uso exclusivo del destinatario.
            Su contenido no debe ser utilizado en forma no autorizada expresamente por el emisor.
            El emisor no acepta responsabilidad por errores u omisiones producidas ni garantiza lo
            transmitido por este medio debido a que puede ser objeto de interpretación,
            alteración, demora, contener virus u otras anomalías. CONSULTATIO
        </p>
        <hr />
        <p class="copyright">©Consultatio - Todos los derechos reservados</p>
    </div>
</body', 1)
GO

-- INSERT NotificationType
DECLARE @LastIdTemplate INT;
SELECT TOP 1 @LastIdTemplate = Id FROM Template ORDER BY Id DESC

INSERT [dbo].[NotificationType] ([DeliveryId], [Description], [TemplateId], [ConfigurationDays], [Discriminator], [CronExpression]) 
VALUES (1, N'Alerta por libre deuda', @LastIdTemplate, 0, N'DebtFree', NULL)

-- INSERT NotificationTypeXRole
DECLARE @LastIdNotificationType INT;
SELECT TOP 1 @LastIdNotificationType = Id FROM NotificationType ORDER BY Id DESC

INSERT [dbo].[NotificationTypeXRole] ([RoleId], [NotificationTypeId]) 
VALUES (N'Admin', @LastIdNotificationType)

INSERT [dbo].[NotificationTypeXRole] ([RoleId], [NotificationTypeId]) 
VALUES (N'CuentasACobrar', @LastIdNotificationType)
