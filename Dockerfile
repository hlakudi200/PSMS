FROM mcr.microsoft.com/dotnet/sdk:9.0.201 AS build

WORKDIR /app

COPY backend/psms.sln ./
COPY backend/src/ ./src/

RUN dotnet restore ./src/psms.Web.Host/psms.Web.Host.csproj
RUN dotnet publish ./src/psms.Web.Host/psms.Web.Host.csproj -c Release -o /app/publish

# The migrator ships alongside the host so the container can bring the database
# up to date before serving. Without this, a release whose code expects a new
# column starts fine and then fails every query that touches it — which is what
# happened when AddReportPdfObjectKey shipped unapplied and Report/GetAll
# returned 500 in production while Student/GetAll was fine.
RUN dotnet restore ./src/psms.Migrator/psms.Migrator.csproj
RUN dotnet publish ./src/psms.Migrator/psms.Migrator.csproj -c Release -o /app/publish-migrator

FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app/publish .
COPY --from=build /app/publish-migrator ./migrator

ENV ASPNETCORE_ENVIRONMENT=Docker
ENV ASPNETCORE_URLS=http://+:80
EXPOSE 80

# Migrate, then serve.
#
# The migrator reads ConnectionStrings__Default from the environment, the same
# way the host does, so it needs no configuration of its own. `-q` makes it
# exit 0 on success and 1 on failure, and a failure stops the container rather
# than starting an app that will 500 on the first query against the new schema.
# The database is the thing you most want to fail loudly.
#
# Set SKIP_MIGRATIONS=1 to start without migrating — for rolling an older image
# back, where the code predates the current schema and re-running migrations is
# not what you want.
#
# Written inline rather than as an entrypoint script on purpose: a .sh authored
# on Windows arrives with CRLF line endings and the container dies with a
# confusing "no such file or directory".
ENTRYPOINT ["/bin/sh", "-c", "\
if [ \"$SKIP_MIGRATIONS\" = \"1\" ]; then \
  echo 'SKIP_MIGRATIONS=1 - starting without applying migrations.'; \
else \
  echo 'Applying database migrations...'; \
  dotnet /app/migrator/psms.Migrator.dll -q || { echo 'Migration failed - refusing to start.'; exit 1; }; \
  echo 'Migrations applied.'; \
fi; \
exec dotnet /app/psms.Web.Host.dll"]
