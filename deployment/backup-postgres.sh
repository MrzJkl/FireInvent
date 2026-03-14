#!/usr/bin/env bash
set -euo pipefail

# Host backup script for PostgreSQL containers used by API and Keycloak.
# Default behavior: load .env from deployment directory if present.
# DB dump credentials are read from each running DB container environment,
# which is typically populated from .env at container startup.

ENV_FILE="${ENV_FILE:-./.env}"
LOAD_ENV_FILE=1

CLI_BACKUP_ROOT=""
CLI_RETENTION_DAYS=""
CLI_SKIP_CLEANUP=""

BACKUP_ROOT=""
RETENTION_DAYS=""
SKIP_CLEANUP=""

usage() {
  cat <<'EOF'
Usage: ./backup-postgres.sh [options]

Options:
  -o, --output DIR         Backup output directory (default: ./backups)
  -r, --retention DAYS     Delete .dump files older than DAYS (default: 14)
  -e, --env-file FILE      Load environment variables from FILE (default: ./.env)
      --no-env-file        Do not load any env file
      --skip-cleanup       Do not delete old backups
  -h, --help               Show this help

Environment overrides:
  ENV_FILE, BACKUP_ROOT, RETENTION_DAYS, SKIP_CLEANUP
  API_DB_CONTAINER, KEYCLOAK_DB_CONTAINER

Examples:
  ./backup-postgres.sh
  ./backup-postgres.sh --output /srv/backups/fireinvent --retention 30
  ./backup-postgres.sh --env-file /opt/fireinvent/.env
  SKIP_CLEANUP=1 ./backup-postgres.sh
EOF
}

while [[ $# -gt 0 ]]; do
  case "$1" in
    -o|--output)
      CLI_BACKUP_ROOT="$2"
      shift 2
      ;;
    -r|--retention)
      CLI_RETENTION_DAYS="$2"
      shift 2
      ;;
    -e|--env-file)
      ENV_FILE="$2"
      shift 2
      ;;
    --no-env-file)
      LOAD_ENV_FILE=0
      shift
      ;;
    --skip-cleanup)
      CLI_SKIP_CLEANUP=1
      shift
      ;;
    -h|--help)
      usage
      exit 0
      ;;
    *)
      echo "Unknown option: $1" >&2
      usage
      exit 1
      ;;
  esac
done

if [[ "$LOAD_ENV_FILE" == "1" && -f "$ENV_FILE" ]]; then
  set -a
  # shellcheck disable=SC1090
  source "$ENV_FILE"
  set +a
fi

BACKUP_ROOT="${CLI_BACKUP_ROOT:-${BACKUP_ROOT:-./backups}}"
RETENTION_DAYS="${CLI_RETENTION_DAYS:-${RETENTION_DAYS:-14}}"
SKIP_CLEANUP="${CLI_SKIP_CLEANUP:-${SKIP_CLEANUP:-0}}"

API_DB_CONTAINER="${API_DB_CONTAINER:-fireinvent_db_api}"
KEYCLOAK_DB_CONTAINER="${KEYCLOAK_DB_CONTAINER:-fireinvent_db_keycloak}"

if ! command -v docker >/dev/null 2>&1; then
  echo "Error: docker is not installed or not in PATH." >&2
  exit 1
fi

if ! [[ "$RETENTION_DAYS" =~ ^[0-9]+$ ]]; then
  echo "Error: retention days must be a non-negative integer." >&2
  exit 1
fi

mkdir -p "$BACKUP_ROOT"

timestamp="$(date +%Y%m%d_%H%M%S)"

assert_container_running() {
  local container="$1"
  local running

  if ! running="$(docker inspect --format '{{.State.Running}}' "$container" 2>/dev/null)"; then
    echo "Error: container '$container' not found." >&2
    exit 1
  fi

  if [[ "$running" != "true" ]]; then
    echo "Error: container '$container' is not running." >&2
    exit 1
  fi
}

backup_container() {
  local container="$1"
  local label="$2"
  local db_name
  local backup_file
  local stderr_file

  assert_container_running "$container"

  db_name="$(docker exec "$container" printenv POSTGRES_DB | tr -d '\r' | tr -d '\n')"
  if [[ -z "$db_name" ]]; then
    echo "Error: could not read POSTGRES_DB from '$container'." >&2
    exit 1
  fi

  backup_file="$BACKUP_ROOT/${label}_${db_name}_${timestamp}.sql"
  stderr_file="$backup_file.stderr.log"

  echo "Creating backup: $backup_file"

  if ! docker exec "$container" sh -lc 'export PGPASSWORD="$POSTGRES_PASSWORD"; pg_dump -U "$POSTGRES_USER" -d "$POSTGRES_DB" -Fp' >"$backup_file" 2>"$stderr_file"; then
    rm -f "$backup_file"
    echo "Error: backup failed for '$container'." >&2
    if [[ -s "$stderr_file" ]]; then
      cat "$stderr_file" >&2
    fi
    exit 1
  fi

  rm -f "$stderr_file"
}

backup_container "$API_DB_CONTAINER" "api"
backup_container "$KEYCLOAK_DB_CONTAINER" "keycloak"

if [[ "$SKIP_CLEANUP" != "1" && "$RETENTION_DAYS" -gt 0 ]]; then
  find "$BACKUP_ROOT" -maxdepth 1 -type f -name '*.sql' -mtime "+$RETENTION_DAYS" -delete
fi

echo "Backups completed successfully in: $BACKUP_ROOT"
