FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src
COPY ["src/psms.Web.Host/psms.Web.Host.csproj", "src/psms.Web.Host/"]
COPY ["src/psms.Web.Core/psms.Web.Core.csproj", "src/psms.Web.Core/"]
COPY ["src/psms.Application/psms.Application.csproj", "src/psms.Application/"]
COPY ["src/psms.Core/psms.Core.csproj", "src/psms.Core/"]
COPY ["src/psms.EntityFrameworkCore/psms.EntityFrameworkCore.csproj", "src/psms.EntityFrameworkCore/"]
WORKDIR "/src/src/psms.Web.Host"
RUN dotnet restore 

WORKDIR /src
COPY ["src/psms.Web.Host", "src/psms.Web.Host"]
COPY ["src/psms.Web.Core", "src/psms.Web.Core"]
COPY ["src/psms.Application", "src/psms.Application"]
COPY ["src/psms.Core", "src/psms.Core"]
COPY ["src/psms.EntityFrameworkCore", "src/psms.EntityFrameworkCore"]
WORKDIR "/src/src/psms.Web.Host"
RUN dotnet publish -c Release -o /publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:8.0
EXPOSE 80
WORKDIR /app
COPY --from=build /publish .
ENTRYPOINT ["dotnet", "psms.Web.Host.dll"]
