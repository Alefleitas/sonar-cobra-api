using Microsoft.EntityFrameworkCore.Migrations;
using nordelta.cobra.webapi.Models;

namespace nordelta.cobra.webapi.Migrations
{
    public partial class AddNewTemplateForQuotationBot : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData("Template", new[] { "Subject", "Description", "HtmlBody", "Disabled" }, new object[]
            {
                 "COBRA - Cotizaciones del día" ,
                 TemplateDescription.Quotations ,
                 @"<!doctype html>
                    <html xmlns=""http://www.w3.org/1999/xhtml"" xmlns:v=""urn:schemas-microsoft-com:vml"" xmlns:o=""urn:schemas-microsoft-com:office:office""><head>
                        <title>
                          
                        </title>
                        <!--[if !mso]><!-- -->
                        <meta http-equiv=""X-UA-Compatible"" content=""IE=edge"">
                        <!--<![endif]-->
                        <meta http-equiv=""Content-Type"" content=""text/html; charset=UTF-8"">
                        <meta name=""viewport"" content=""width=device-width, initial-scale=1"">
                        <style type=""text/css"">
                          #outlook a { padding:0; }
                          body { margin:0;padding:0;-webkit-text-size-adjust:100%;-ms-text-size-adjust:100%; }
                          table, td { border-collapse:collapse;mso-table-lspace:0pt;mso-table-rspace:0pt; }
                          img { border:0;height:auto;line-height:100%; outline:none;text-decoration:none;-ms-interpolation-mode:bicubic; }
                          p { display:block;margin:13px 0; }
                        </style>
                        <!--[if mso]>
                        <xml>
                        <o:OfficeDocumentSettings>
                          <o:AllowPNG/>
                          <o:PixelsPerInch>96</o:PixelsPerInch>
                        </o:OfficeDocumentSettings>
                        </xml>
                        <![endif]-->
                        <!--[if lte mso 11]>
                        <style type=""text/css"">
                          .outlook-group-fix { width:100% !important; }
                        </style>
                        <![endif]-->
                        
                        
                        <style type=""text/css"">
                          @media only screen and (max-width:480px) {
                            .mj-column-per-100 {
                              width: 100% !important;
                              max-width: 100%;
                            }
                        
                            .mj-column-per-50 {
                              width: 50% !important;
                              max-width: 50%;
                            }
                          }
                        
                          #quotations {
                            background: #FFFFFF;
                            background-color: #FFFFFF;
                            margin: 0px auto;
                            max-width: 600px;
                          }
                        
                          #quotations table {
                            align: center;
                            border: 0;
                            cellpadding: 0;
                            cellspacing: 0;
                            role: ""presentation"";
                            width: 100%;
                          }
                        
                          #quotations td {
                            direction: ltr;
                            font-size: 0px;
                            padding: 0px 0px 0px 0px;
                            text-align: center;
                          }
                        
                        
                          #quotations p {
                            font-family: Itim, cursive;
                            font-size: 11px;
                            line-height: 1.5;
                            text-align: left;
                            color: #FFFFFF;
                          }
                        
                          #quotations span {
                            font-family: Helvetica, sans-serif;
                            font-size: 16px;
                            color: #34495e;
                          }
                        
                          #quotColumn {
                            font-size: 0px;
                            text-align: left;
                            direction: ltr;
                            display: inline-block;
                            vertical-align: top;
                            width: 50%;
                          }
                        
                          #quotColumn td {
                            align: ""left"";
                            font-size: 0px;
                            padding: 10px 25px 10px 26px;
                            word-break: break-word;
                          }
                        
                          #quotColum border {
                            font-size: 0px;
                            padding: 10px 10px;
                            padding-top: 0px;
                            word-break: break-word;
                          }
                        
                          #separator {
                            background: #FFFFFF;
                            background-color: #FFFFFF;
                            margin: 0px auto;
                            max-width: 600px;
                          }
                        
                          #separator table {
                            align: center;
                            border: 0;
                            cellpadding: 0;
                            cellspacing: 0;
                            role: ""presentation"";
                            width: 100%;
                            vertical-align: top;
                          }
                        
                          #separator td {
                            direction: ltr;
                            font-size: 0px;
                            padding: 0px 0px 0px 0px;
                            text-align: center;
                          }
                        
                          #separator p {
                            font-family: Itim, cursive;
                            border-top: solid 1px #D9D9D9;
                            font-size: 1;
                            margin: 0px auto;
                            width: 100%;
                          }
                        
                          #sepColumn {
                            font-size: 0px;
                            text-align: left;
                            direction: ltr;
                            display: inline-block;
                            vertical-align: top;
                            width: 100%;
                          }
                        </style>
                    
                  
                        <style type=""text/css"">
                        
                        

                    @media only screen and (max-width:480px) {
                      table.full-width-mobile { width: 100% !important; }
                      td.full-width-mobile { width: auto !important; }
                    }
                  
                        </style>
                        <style type=""text/css"">.hide_on_mobile { display: none !important;} 
                        @media only screen and (min-width: 480px) { .hide_on_mobile { display: block !important;} }
                        .hide_section_on_mobile { display: none !important;} 
                        @media only screen and (min-width: 480px) { 
                            .hide_section_on_mobile { 
                                display: table !important;
                            } 

                            div.hide_section_on_mobile { 
                                display: block !important;
                            }
                        }
                        .hide_on_desktop { display: block !important;} 
                        @media only screen and (min-width: 480px) { .hide_on_desktop { display: none !important;} }
                        .hide_section_on_desktop { 
                            display: table !important;
                            width: 100%;
                        } 
                        @media only screen and (min-width: 480px) { .hide_section_on_desktop { display: none !important;} }
                        
                          p, h1, h2, h3 {
                              margin: 0px;
                          }

                          ul, li, ol {
                            font-size: 11px;
                            font-family: Ubuntu, Helvetica, Arial;
                          }

                          a {
                              text-decoration: none;
                              color: inherit;
                          }

                          @media only screen and (max-width:480px) {

                            .mj-column-per-100 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-100 > .mj-column-per-75 { width:75%!important; max-width:75%!important; }
                            .mj-column-per-100 > .mj-column-per-60 { width:60%!important; max-width:60%!important; }
                            .mj-column-per-100 > .mj-column-per-50 { width:50%!important; max-width:50%!important; }
                            .mj-column-per-100 > .mj-column-per-40 { width:40%!important; max-width:40%!important; }
                            .mj-column-per-100 > .mj-column-per-33 { width:33.333333%!important; max-width:33.333333%!important; }
                            .mj-column-per-100 > .mj-column-per-25 { width:25%!important; max-width:25%!important; }

                            .mj-column-per-100 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-75 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-60 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-50 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-40 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-33 { width:100%!important; max-width:100%!important; }
                            .mj-column-per-25 { width:100%!important; max-width:100%!important; }
                        }</style>
                        
                      </head>
                      <body style=""background-color:#FFFFFF;"">
                        
                        
                      <div style=""background-color:#FFFFFF;"">
                        
                      
                      <!--[if mso | IE]>
                      <table
                         align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class="""" style=""width:600px;"" width=""600""
                      >
                        <tr>
                          <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
                      <![endif]-->
                    
                      
                      <div style=""background:#173254;background-color:#173254;margin:0px auto;max-width:600px;"">
                        
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#173254;background-color:#173254;width:100%;"">
                          <tbody>
                            <tr>
                              <td style=""direction:ltr;font-size:0px;padding:9px 0px 9px 0px;text-align:center;"">
                                <!--[if mso | IE]>
                                  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                
                        <tr>
                      
                            <td
                               class="""" style=""vertical-align:top;width:600px;""
                            >
                          <![endif]-->
                            
                      <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        
                            <tbody><tr>
                              <td align=""left"" class=""hide_on_mobile"" style=""font-size:0px;padding:0px 0px 0px 10px;word-break:break-word;"">
                                
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""border-collapse:collapse;border-spacing:0px;"">
                        <tbody>
                          <tr>
                            <td style=""width:195px;"">
                              
                      <img height=""auto"" src=""https://19687661.fs1.hubspotusercontent-na1.net/hubfs/19687661/%5BDise%C3%B1o%5D%20Logos%20y%20manuales%20de%20marca/Consultatio/Logo%20Consultatio.png"" style=""border:0;display:block;outline:none;text-decoration:none;height:auto;width:100%;font-size:13px;"" width=""195"">
                    
                            </td>
                          </tr>
                        </tbody>
                      </table>
                    
                              </td>
                            </tr>
                          
                      </tbody></table>
                    
                      </div>
                    
                          <!--[if mso | IE]>
                            </td>
                          
                        </tr>
                      
                                  </table>
                                <![endif]-->
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        
                      </div>
                    
                      
                      <!--[if mso | IE]>
                          </td>
                        </tr>
                      </table>
                      
                      <table
                         align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class="""" style=""width:600px;"" width=""600""
                      >
                        <tr>
                          <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
                      <![endif]-->
                    
                      
                      <div style=""background:#;background-color:#;margin:0px auto;max-width:600px;"">
                        
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#;background-color:#;width:100%;"">
                          <tbody>
                            <tr>
                              <td style=""direction:ltr;font-size:0px;padding:0px 0px 0px 0px;text-align:center;"">
                                <!--[if mso | IE]>
                                  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                
                        <tr>
                      
                            <td
                               class="""" style=""vertical-align:top;width:600px;""
                            >
                          <![endif]-->
                            
                      <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        
                            <tbody><tr>
                              <td style=""font-size:0px;word-break:break-word;"">
                                
                      
                    <!--[if mso | IE]>
                    
                        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0""><tr><td height=""30"" style=""vertical-align:top;height:30px;"">
                      
                    <![endif]-->
                  
                      <div style=""height:30px;"">
                        &nbsp;
                      </div>
                      
                    <!--[if mso | IE]>
                    
                        </td></tr></table>
                      
                    <![endif]-->
                  
                    
                              </td>
                            </tr>
                          
                            <tr>
                              <td align=""left"" style=""font-size:0px;padding:10px 25px 0px 26px;word-break:break-word;"">
                                
                      <div style=""font-family:Itim, cursive;font-size:11px;line-height:1.5;text-align:left;color:#FFFFFF;""><p style=""font-family: Itim, cursive; font-size: 11px; text-align: left;""><span style=""font-family: Helvetica, sans-serif; font-size: 30px;""><span style=""color: #173254;"">Tipo de cambio</span></span></p></div>
                    
                              </td>
                            </tr>
                          
                      </tbody></table>
                    
                      </div>
                    
                          <!--[if mso | IE]>
                            </td>
                          
                        </tr>
                      
                                  </table>
                                <![endif]-->
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        
                      </div>
                    
                      
                      <!--[if mso | IE]>
                          </td>
                        </tr>
                      </table>
                      
                      <table
                         align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class="""" style=""width:600px;"" width=""600""
                      >
                        <tr>
                          <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
                      <![endif]-->
                    
                      
                      <div style=""background:#FFFFFF;background-color:#FFFFFF;margin:0px auto;max-width:600px;"">
                        
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#FFFFFF;background-color:#FFFFFF;width:100%;"">
                          <tbody>
                            <tr>
                              <td style=""direction:ltr;font-size:0px;padding:0px 0px 0px 0px;text-align:center;"">
                                <!--[if mso | IE]>
                                  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                
                        <tr>
                      
                            <td
                               class="""" style=""vertical-align:top;width:600px;""
                            >
                          <![endif]-->
                            
                      <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        
                            <tbody><tr>
                              <td align=""left"" style=""font-size:0px;padding: 0px 25px 10px 26px;word-break:break-word;"">
                                
                      <div style=""font-family:Itim, cursive;font-size:11px;line-height:1.5;text-align:left;color:#FFFFFF;""><p style=""font-family: Itim, cursive; font-size: 11px; text-align: left;""><span style=""font-family: Helvetica, sans-serif; font-size: 16px; color: #bababa;"">Cotización vigente al {{DATE_NOW}}*</span></p></div>
                    
                              </td>
                            </tr>
                          
                      </tbody></table>
                    
                      </div>
                    
                          <!--[if mso | IE]>
                            </td>
                          
                        </tr>
                      
                                  </table>
                                <![endif]-->
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        
                      </div>
                    
                      
                      <!--[if mso | IE]>
                          </td>
                        </tr>
                      </table>
                      
                      <table
                         align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class="""" style=""width:600px;"" width=""600""
                      >
                        <tr>
                          <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
                      <![endif]-->
                    
                      
                      <div style=""background:#FFFFFF;background-color:#FFFFFF;margin:0px auto;max-width:600px;"">
                        
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#FFFFFF;background-color:#FFFFFF;width:100%;"">
                          <tbody>
                            <tr>
                              <td style=""direction:ltr;font-size:0px;padding:0px 0px 0px 0px;text-align:center;"">
                                <!--[if mso | IE]>
                                  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                
                        <tr>
                      
                            <td
                               class="""" style=""vertical-align:top;width:600px;""
                            >
                          <![endif]-->
                            
                      <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        
                            <tbody><tr>
                              <td style=""font-size:0px;padding:10px 10px;padding-top:0px;word-break:break-word;padding-bottom: 0px!important;"">
                                
                      <p style=""font-family: Itim, cursive; border-top: solid 2px #173254; font-size: 1; margin: 0px auto; width: 100%;"">
                      </p>
                      
                      <!--[if mso | IE]>
                        <table
                           align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" style=""border-top:solid 2px #173254;font-size:1;margin:0px auto;width:580px;"" role=""presentation"" width=""580px""
                        >
                          <tr>
                            <td style=""height:0;line-height:0;"">
                              &nbsp;
                            </td>
                          </tr>
                        </table>
                      <![endif]-->
                    
                    
                              </td>
                            </tr>
                          
                      </tbody></table>
                    
                      </div>
                    
                          <!--[if mso | IE]>
                            </td>
                          
                        </tr>
                      
                                  </table>
                                <![endif]-->
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        
                      </div>
                    
                      
                      {{QUOTATIONS}}
                    
                      
                      <div style=""background:#FFFFFF;background-color:#FFFFFF;margin:0px auto;max-width:600px;"">
                        
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#FFFFFF;background-color:#FFFFFF;width:100%;"">
                          <tbody>
                            <tr>
                              <td style=""direction:ltr;font-size:0px;padding:0px 0px 0px 0px;text-align:center;"">
                                <!--[if mso | IE]>
                                  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                
                        <tr>
                      
                            <td
                               class="""" style=""vertical-align:top;width:600px;""
                            >
                          <![endif]-->
                            
                      <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        
                            <tbody><tr>
                              <td align=""left"" style=""font-size:0px;padding:0px 25px 10px 26px;word-break:break-word;"">
                                
                      <div style=""font-family:Itim, cursive;font-size:11px;line-height:1.5;text-align:left;color:#FFFFFF;""><p style=""font-family: Itim, cursive; font-size: 11px; text-align: left;""><span style=""font-family: Helvetica, sans-serif; font-size: 14px; color: #bababa;"">*Puede variar sin previo aviso.</span></p></div>
                    
                              </td>
                            </tr>
                          
                            <tr>
                              <td style=""font-size:0px;word-break:break-word;"">
                                
                      
                    <!--[if mso | IE]>
                    
                        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0""><tr><td height=""40"" style=""vertical-align:top;height:40px;"">
                      
                    <![endif]-->
                  
                      <div style=""height:40px;"">
                        &nbsp;
                      </div>
                      
                    <!--[if mso | IE]>
                    
                        </td></tr></table>
                      
                    <![endif]-->
                  
                    
                              </td>
                            </tr>
                          
                      </tbody></table>
                    
                      </div>
                    
                          <!--[if mso | IE]>
                            </td>
                          
                        </tr>
                      
                                  </table>
                                <![endif]-->
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        
                      </div>
                    
                      
                      <!--[if mso | IE]>
                          </td>
                        </tr>
                      </table>
                      
                      <table
                         align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" class="""" style=""width:600px;"" width=""600""
                      >
                        <tr>
                          <td style=""line-height:0px;font-size:0px;mso-line-height-rule:exactly;"">
                      <![endif]-->
                    
                      
                      <div style=""background:#173254;background-color:#173254;margin:0px auto;max-width:600px;"">
                        
                        <table align=""center"" border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""background:#173254;background-color:#173254;width:100%;"">
                          <tbody>
                            <tr>
                              <td style=""direction:ltr;font-size:0px;padding:0px 0px 0px 0px;text-align:center;"">
                                <!--[if mso | IE]>
                                  <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0"">
                                
                        <tr>
                      
                            <td
                               class="""" style=""vertical-align:top;width:600px;""
                            >
                          <![endif]-->
                            
                      <div class=""mj-column-per-100 outlook-group-fix"" style=""font-size:0px;text-align:left;direction:ltr;display:inline-block;vertical-align:top;width:100%;"">
                        
                      <table border=""0"" cellpadding=""0"" cellspacing=""0"" role=""presentation"" style=""vertical-align:top;"" width=""100%"">
                        
                            <tbody><tr>
                              <td style=""font-size:0px;word-break:break-word;"">
                                
                      
                    <!--[if mso | IE]>
                    
                        <table role=""presentation"" border=""0"" cellpadding=""0"" cellspacing=""0""><tr><td height=""15"" style=""vertical-align:top;height:15px;"">
                      
                    <![endif]-->
                  
                      <div style=""height:15px;"">
                        &nbsp;
                      </div>
                      
                    <!--[if mso | IE]>
                    
                        </td></tr></table>
                      
                    <![endif]-->
                  
                    
                              </td>
                            </tr>
                          
                      </tbody></table>
                    
                      </div>
                    
                          <!--[if mso | IE]>
                            </td>
                          
                        </tr>
                      
                                  </table>
                                <![endif]-->
                              </td>
                            </tr>
                          </tbody>
                        </table>
                        
                      </div>
                    
                      
                      <!--[if mso | IE]>
                          </td>
                        </tr>
                      </table>
                      <![endif]-->
                    
                    
                      </div>
                    
                      
                    </body></html>
                ",
                 false 
            });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql($"DELETE FROM [dbo].Template WHERE Description = '{TemplateDescription.Quotations}'");
        }
    }
}
