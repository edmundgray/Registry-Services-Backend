# SQL Server 2022 that restores registry.bak on first start (when RegistryDatabase doesn't exist yet)
FROM mcr.microsoft.com/mssql/server:2022-latest

# Root so SQL Server can write to a host-mounted volume (e.g. Railway volumes are root-owned)
USER root

ENV ACCEPT_EULA=Y \
    MSSQL_PID=Express

COPY registry.bak /backup/registry.bak
COPY entrypoint.sh /usr/local/bin/entrypoint.sh
RUN sed -i 's/\r$//' /usr/local/bin/entrypoint.sh && chmod +x /usr/local/bin/entrypoint.sh

EXPOSE 1433
ENTRYPOINT ["/usr/local/bin/entrypoint.sh"]
