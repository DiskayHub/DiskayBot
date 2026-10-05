#!/bin/bash
set -euo pipefail

# ─────────────────────────────────────────────
#  Colors
# ─────────────────────────────────────────────
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
CYAN='\033[0;36m'
BOLD='\033[1m'
RESET='\033[0m'

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"

log_step() {
    echo -e "\n${BLUE}${BOLD}▶ $1${RESET}"
}

log_ok() {
    echo -e "  ${GREEN}✔ $1${RESET}"
}

log_warn() {
    echo -e "  ${YELLOW}⚠ $1${RESET}"
}

log_err() {
    echo -e "  ${RED}✘ $1${RESET}"
}

log_info() {
    echo -e "  ${CYAN}→ $1${RESET}"
}

echo -e "\n${BOLD}════════════════════════════════════════${RESET}"
echo -e "${BOLD}         DiskayHub Deploy Script        ${RESET}"
echo -e "${BOLD}════════════════════════════════════════${RESET}"

# ─────────────────────────────────────────────
#  Arguments
# ─────────────────────────────────────────────
# Values passed here are injected into the diskayBot container environment
# through DiskayBot/DiskayBot.Application/.env, which is generated from them
# in Step 1. All options are required.

ARG_LOGIN=""
ARG_PASSWORD=""
ARG_ADMIN_ID=""
ARG_TOKEN=""
ARG_REDIS_PASSWORD=""
ARG_POSTGRES_PASSWORD=""

usage() {
    cat <<EOF
Usage: $(basename "$0") [options]

  -l, --login <value>       ScheduleClient login    -> ScheduleClient__login
  -p, --password <value>    ScheduleClient password -> ScheduleClient__password
  -a, --admin-id <value>    Telegram admin id       -> Admin__AdminId
  -t, --token <value>       Telegram bot API token  -> TelegramBot__Token
  -r, --redis-password <value>
                            Redis password          -> Redis__ConnectionString, REDIS_PASSWORD
  -d, --db-password <value>
                            Postgres password       -> POSTGRES_PASSWORD, DiskayMemory connection string
  -h, --help                show this help
EOF
}

while [ $# -gt 0 ]; do
    case "$1" in
        -l|--login)
            [ $# -ge 2 ] || { log_err "$1 requires a value"; exit 1; }
            ARG_LOGIN="$2"; shift 2 ;;
        -p|--password)
            [ $# -ge 2 ] || { log_err "$1 requires a value"; exit 1; }
            ARG_PASSWORD="$2"; shift 2 ;;
        -a|--admin-id)
            [ $# -ge 2 ] || { log_err "$1 requires a value"; exit 1; }
            ARG_ADMIN_ID="$2"; shift 2 ;;
        -t|--token)
            [ $# -ge 2 ] || { log_err "$1 requires a value"; exit 1; }
            ARG_TOKEN="$2"; shift 2 ;;
        -r|--redis-password)
            [ $# -ge 2 ] || { log_err "$1 requires a value"; exit 1; }
            ARG_REDIS_PASSWORD="$2"; shift 2 ;;
        -d|--db-password)
            [ $# -ge 2 ] || { log_err "$1 requires a value"; exit 1; }
            ARG_POSTGRES_PASSWORD="$2"; shift 2 ;;
        -h|--help)
            usage; exit 0 ;;
        *)
            log_err "unknown argument: $1"
            usage
            exit 1 ;;
    esac
done

MISSING=()
[ -n "$ARG_LOGIN" ]    || MISSING+=("--login")
[ -n "$ARG_PASSWORD" ] || MISSING+=("--password")
[ -n "$ARG_ADMIN_ID" ] || MISSING+=("--admin-id")
[ -n "$ARG_TOKEN" ]    || MISSING+=("--token")
[ -n "$ARG_REDIS_PASSWORD" ] || MISSING+=("--redis-password")
[ -n "$ARG_POSTGRES_PASSWORD" ] || MISSING+=("--db-password")

if [ ${#MISSING[@]} -gt 0 ]; then
    log_err "missing required argument(s): ${MISSING[*]}"
    usage
    exit 1
fi

# Passwords end up unquoted in compose commands and connection strings,
# where spaces, quotes, ',' or ';' would silently break them
INVALID=()
[[ "$ARG_REDIS_PASSWORD" =~ ^[A-Za-z0-9]+$ ]] || INVALID+=("--redis-password")
[[ "$ARG_POSTGRES_PASSWORD" =~ ^[A-Za-z0-9]+$ ]] || INVALID+=("--db-password")

if [ ${#INVALID[@]} -gt 0 ]; then
    log_err "only letters and digits are allowed in: ${INVALID[*]}"
    exit 1
fi

# ─────────────────────────────────────────────
#  Step 1: Sync repositories
# ─────────────────────────────────────────────
log_step "Step 1: Syncing repositories"

sync_repo() {
    local dir="$1"
    local url="$2"
    local path="$SCRIPT_DIR/$dir"

    if [ -d "$path" ]; then
        log_info "$dir already exists — pulling latest changes..."
        git -C "$path" pull origin master
        log_ok "$dir updated"
    else
        log_info "$dir not found — cloning from $url..."
        git clone "$url" "$path"
        log_ok "$dir cloned"
    fi
}

sync_repo "DiskayBot"    "git@github.com:DiskayHub/DiskayBot.git"
sync_repo "DiskayMemory" "git@github.com:DiskayHub/DiskayMemory.git"

# docker-compose.yml lives in the DiskayBot repo but runs from DiskayHub root
cp "$SCRIPT_DIR/DiskayBot/docker-compose.yml" "$SCRIPT_DIR/docker-compose.yml"
log_ok "docker-compose.yml copied from DiskayBot"

# Generate the bot .env from scratch using only the CLI arguments
ENV_DST="$SCRIPT_DIR/DiskayBot/DiskayBot.Application/.env"
{
    printf '%s=%s\n' "ScheduleClient__login"    "$ARG_LOGIN"
    printf '%s=%s\n' "ScheduleClient__password" "$ARG_PASSWORD"
    printf '%s=%s\n' "Admin__AdminId"            "$ARG_ADMIN_ID"
    printf '%s=%s\n' "TelegramBot__Token"        "$ARG_TOKEN"
    printf '%s=%s\n' "Redis__ConnectionString"   "redis:6379,password=${ARG_REDIS_PASSWORD},abortConnect=false"
} > "$ENV_DST"
log_ok ".env generated at DiskayBot/DiskayBot.Application/ from arguments"

# ─────────────────────────────────────────────
#  Step 2: Database backup
# ─────────────────────────────────────────────
log_step "Step 2: Creating database backup"

BACKUP_DIR="$SCRIPT_DIR/backup"
mkdir -p "$BACKUP_DIR"
log_info "Backup directory: $BACKUP_DIR"

if docker ps --format '{{.Names}}' | grep -q '^diskay_postgres$'; then
    TIMESTAMP=$(date +"%Y%m%d_%H%M%S")
    BACKUP_FILE="$BACKUP_DIR/diskay_postgres_$TIMESTAMP.sql"
    log_info "Container diskay_postgres is running — dumping database..."
    docker exec diskay_postgres pg_dumpall -U postgres > "$BACKUP_FILE"
    log_ok "Backup saved: backup/diskay_postgres_$TIMESTAMP.sql"
else
    log_warn "Container diskay_postgres is not running — skipping backup"
fi

# ─────────────────────────────────────────────
#  Step 3: Docker Compose up
# ─────────────────────────────────────────────
log_step "Step 3: Starting services with Docker Compose"

COMPOSE_FILE="$SCRIPT_DIR/docker-compose.yml"

if [ ! -f "$COMPOSE_FILE" ]; then
    log_err "docker-compose.yml not found in $SCRIPT_DIR"
    exit 1
fi

# Shell environment takes precedence over .env in compose interpolation,
# so redis and postgres start with the passwords the services were given
export REDIS_PASSWORD="$ARG_REDIS_PASSWORD"
export POSTGRES_PASSWORD="$ARG_POSTGRES_PASSWORD"

# POSTGRES_PASSWORD only applies when the database is first created, so the
# password is synced explicitly before DiskayMemory starts and runs migrations
log_info "Starting postgres and syncing its password..."
docker compose -f "$COMPOSE_FILE" up -d postgres
for i in $(seq 1 30); do
    [ "$(docker inspect -f '{{.State.Health.Status}}' diskay_postgres 2>/dev/null)" = "healthy" ] && break
    sleep 2
done
if [ "$(docker inspect -f '{{.State.Health.Status}}' diskay_postgres 2>/dev/null)" != "healthy" ]; then
    log_err "postgres did not become healthy in 60s"
    exit 1
fi
# Sent via stdin so the password does not show up in the process list
printf "ALTER USER postgres WITH PASSWORD '%s';\n" "$ARG_POSTGRES_PASSWORD" \
    | docker exec -i diskay_postgres psql -U postgres -q -v ON_ERROR_STOP=1
log_ok "postgres password synced"

log_info "Running docker compose up --build -d..."
docker compose -f "$COMPOSE_FILE" up --build -d
log_ok "Docker Compose started"

# ─────────────────────────────────────────────
#  Step 4: Health check
# ─────────────────────────────────────────────
log_step "Step 4: Checking service health"

# Wait for containers to stabilize
log_info "Waiting 5 seconds for containers to stabilize..."
sleep 5

FAILED=0

# Check container status via docker compose ps
log_info "Checking container statuses..."
while IFS= read -r line; do
    NAME=$(echo "$line" | awk '{print $1}')
    STATUS=$(echo "$line" | awk '{$1=""; print $0}' | xargs)

    if echo "$STATUS" | grep -qiE 'up|running|healthy'; then
        log_ok "$NAME — $STATUS"
    else
        log_err "$NAME — $STATUS"
        FAILED=$((FAILED + 1))
    fi
done < <(docker compose -f "$COMPOSE_FILE" ps --format 'table {{.Service}}\t{{.Status}}' | tail -n +2)

# Check ports for services that expose them
check_port() {
    local service="$1"
    local port="$2"
    local retries=5
    local wait=3

    for i in $(seq 1 $retries); do
        if (echo > /dev/tcp/127.0.0.1/"$port") 2>/dev/null; then
            log_ok "$service — port $port is open"
            return 0
        fi
        log_info "$service — port $port not ready, retry $i/$retries..."
        sleep "$wait"
    done

    log_err "$service — port $port is not reachable after $((retries * wait))s"
    FAILED=$((FAILED + 1))
}

echo ""
log_info "Checking exposed ports..."
check_port "diskay_memory" 8080
check_port "postgres"      5432
check_port "redis"         6379

# For diskayBot (no ports) — check it's not in a restart loop
echo ""
log_info "Checking diskayBot for restart loops..."
BOT_CONTAINER="diskayBot"
if docker inspect "$BOT_CONTAINER" &>/dev/null; then
    RESTART_COUNT=$(docker inspect --format '{{.RestartCount}}' "$BOT_CONTAINER")
    STATUS=$(docker inspect --format '{{.State.Status}}' "$BOT_CONTAINER")
    RESTARTING=$(docker inspect --format '{{.State.Restarting}}' "$BOT_CONTAINER")

    if [ "$RESTARTING" = "true" ]; then
        log_err "$BOT_CONTAINER — is restarting (RestartCount: $RESTART_COUNT)"
        FAILED=$((FAILED + 1))
    elif [ "$RESTART_COUNT" -gt 2 ]; then
        log_warn "$BOT_CONTAINER — status: $STATUS, RestartCount: $RESTART_COUNT (possible crash loop)"
    else
        log_ok "$BOT_CONTAINER — status: $STATUS, RestartCount: $RESTART_COUNT"
    fi
else
    log_err "$BOT_CONTAINER — container not found"
    FAILED=$((FAILED + 1))
fi

echo ""
if [ "$FAILED" -gt 0 ]; then
    echo -e "${RED}${BOLD}════════════════════════════════════════${RESET}"
    echo -e "${RED}${BOLD}  Deploy finished with $FAILED failing service(s)${RESET}"
    echo -e "${RED}${BOLD}════════════════════════════════════════${RESET}\n"
    exit 1
else
    echo -e "${GREEN}${BOLD}════════════════════════════════════════${RESET}"
    echo -e "${GREEN}${BOLD}       Deploy completed successfully!    ${RESET}"
    echo -e "${GREEN}${BOLD}════════════════════════════════════════${RESET}\n"
fi