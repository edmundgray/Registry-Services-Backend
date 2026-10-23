#!/bin/bash
# Starts SQL Server and, if RegistryDatabase doesn't exist yet, restores it from /backup/registry.bak.
set -u

DB_NAME="RegistryDatabase"
BACKUP="/backup/registry.bak"
SQLCMD=(/opt/mssql-tools18/bin/sqlcmd -C -S localhost -U sa -P "$MSSQL_SA_PASSWORD" -b)

# Trace flag 1800 forces 4K-aligned log IO; without it SQL Server crashes on volumes
# whose reported sector size it doesn't expect (e.g. Railway volumes).
mkdir -p /var/opt/mssql

# One-off recovery switch: wipes ALL data on the volume. Unset it again after use.
if [ "${WIPE_DATA_ON_START:-}" = "true" ]; then
    echo "WIPE_DATA_ON_START=true: deleting /var/opt/mssql contents"
    rm -rf /var/opt/mssql/*
fi
cat > /var/opt/mssql/mssql.conf <<'EOF'
[traceflag]
traceflag0 = 1800
EOF

/opt/mssql/bin/sqlservr -T1800 &
SQL_PID=$!

for _ in $(seq 1 90); do
    "${SQLCMD[@]}" -Q "SELECT 1" > /dev/null 2>&1 && break
    sleep 2
done

exists=$("${SQLCMD[@]}" -h -1 -W -Q "SET NOCOUNT ON; SELECT COUNT(*) FROM sys.databases WHERE name = '$DB_NAME'" | tr -d '[:space:]')

if [ "$exists" = "0" ] && [ -f "$BACKUP" ]; then
    echo "Restoring $DB_NAME from $BACKUP ..."
    # Relocate each logical file in the backup into the default data directory
    moves=$("${SQLCMD[@]}" -h -1 -W -s '|' -Q "SET NOCOUNT ON; RESTORE FILELISTONLY FROM DISK = '$BACKUP'" \
        | awk -F'|' -v db="$DB_NAME" 'NF > 2 {
            ext = ($3 == "L") ? "_log.ldf" : ".mdf";
            if ($3 == "D" && seen++) ext = "_" seen ".ndf";
            printf ", MOVE N'\''%s'\'' TO N'\''/var/opt/mssql/data/%s_%s%s'\''", $1, db, $1, ext
        }')
    if "${SQLCMD[@]}" -Q "RESTORE DATABASE [$DB_NAME] FROM DISK = '$BACKUP' WITH RECOVERY$moves"; then
        echo "Restore complete."
    else
        echo "Restore FAILED." >&2
    fi
else
    echo "$DB_NAME already exists (or no backup present); skipping restore."
fi

wait $SQL_PID
