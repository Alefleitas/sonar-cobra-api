#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["nordelta.service.middle.itau.csproj", "."]
RUN dotnet restore "./nordelta.service.middle.itau.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "nordelta.service.middle.itau.csproj" -c Release -o /app/build

# Agregar especificaciones de dependencias individuales aquí
RUN dotnet add package Microsoft.AspNetCore.Mvc.NewtonsoftJson --version 6.0.5
RUN dotnet add package Microsoft.AspNetCore.Mvc.Versioning --version 5.0.0
RUN dotnet add package Microsoft.VisualStudio.Azure.Containers.Tools.Targets --version 1.17.2
RUN dotnet add package Newtonsoft.Json --version 13.0.1
RUN dotnet add package Serilog.AspNetCore --version 5.0.0
RUN dotnet add package Serilog.Enrichers.Environment --version 2.2.0
RUN dotnet add package Serilog.Enrichers.Thread --version 3.1.0
RUN dotnet add package Serilog.Sinks.Graylog --version 3.0.1
RUN dotnet add package Swashbuckle.AspNetCore --version 6.2.3


FROM build AS publish
RUN dotnet publish "nordelta.service.middle.itau.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Agregar el archivo de configuración específico de producción
COPY appsettings.Development.json appsettings.json

ENTRYPOINT ["dotnet", "nordelta.service.middle.itau.dll"]
