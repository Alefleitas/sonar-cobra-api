FROM mcr.microsoft.com/dotnet/sdk:3.1.410-focal as build

COPY ./ /source
WORKDIR /source/nordelta.cobra.webapi
RUN dotnet restore \
 && dotnet publish --configuration Release -o /publish



#Running aspnet core 3.1 runtime on ubuntu 20.04
FROM mcr.microsoft.com/dotnet/aspnet:3.1.16-focal as runtime

LABEL MAINTAINER rodrigo.vazquez@novit.com.ar
EXPOSE 5000
ENV ASPNETCORE_URLS="http://0.0.0.0:5000"


# By declaring an invalid default value for this variables
# we make sure not to run container in production without 
# giving a --env-file, so it won't take values from web.config by accident
ENV JwtKey "undefined-by-default-in-dockerfile"

ENV ServiceConfiguration__DebitoInmediatoConfiguration__EndpointUrl "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__ArchivosCmlConfiguration__EndpointUrl "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__CuentaServiceConfiguration__EndpointUrl "undefined-by-default-in-dockerfile"

ENV ServiceConfiguration__CertificateSettings__CertificateItems__0__VendorName "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__CertificateSettings__CertificateItems__0__VendorCuit "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__CertificateSettings__CertificateItems__0__Name "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__CertificateSettings__CertificateItems__0__Password "undefined-by-default-in-dockerfile"

ENV ServiceConfiguration__Email__Host "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__Email__Port "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__Email__EnableSsl "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__Email__Email "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__Email__Password "undefined-by-default-in-dockerfile"

ENV ServiceConfiguration__EmailConfig_QuotationBot__SmtpHost "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__EmailConfig_QuotationBot__SmtpPort "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__EmailConfig_QuotationBot__ImapHost "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__EmailConfig_QuotationBot__ImapPort "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__EmailConfig_QuotationBot__EnableSsl "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__EmailConfig_QuotationBot__Email "undefined-by-default-in-dockerfile"
ENV ServiceConfiguration__EmailConfig_QuotationBot__Password "undefined-by-default-in-dockerfile"

ENV ServiceConfiguration__EnableItauMock "undefined-by-default-in-dockerfile"


ENV ConnectionStrings__mssql_db "undefined-by-default-in-dockerfile"
ENV ConnectionStrings__hangfire_db "undefined-by-default-in-dockerfile"

ENV ApiServices__SgcApi__Token "undefined-by-default-in-dockerfile"
ENV ApiServices__SgcApi__Url "undefined-by-default-in-dockerfile"
ENV ApiServices__SgfApi__Token "undefined-by-default-in-dockerfile"
ENV ApiServices__SgfApi__Url "undefined-by-default-in-dockerfile"
ENV ApiServices__SsoApi__Token "undefined-by-default-in-dockerfile"
ENV ApiServices__SsoApi__Token "undefined-by-default-in-dockerfile"
ENV ApiServices__SsoApi__RefreshToken "undefined-by-default-in-dockerfile"
ENV ApiServices__CobraApi__Url "undefined-by-default-in-dockerfile"
ENV ApiServices__HolidaysApi__Url "undefined-by-default-in-dockerfile"



ENV Serilog__MinimumLevel__Default "undefined-by-default-in-dockerfile"
#Graylog
ENV Serilog__WriteTo__0__Args__hostnameOrAddress "undefined-by-default-in-dockerfile"
ENV Serilog__WriteTo__0__Args__port "undefined-by-default-in-dockerfile"
ENV Serilog__WriteTo__0__Args__transportType "undefined-by-default-in-dockerfile"
ENV Serilog__WriteTo__0__Args__MinimumLogEventLevel "undefined-by-default-in-dockerfile"
ENV Serilog__WriteTo__0__Args__restrictedToMinimumLevel "undefined-by-default-in-dockerfile"

COPY --from=build /publish/ /app
WORKDIR /app
ENTRYPOINT [ "dotnet", "nordelta.cobra.webapi.dll"]

