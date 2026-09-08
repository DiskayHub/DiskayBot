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
# through DiskayBot/DiskayBot.Application/.env (see Step 1). Any option that is
# omitted keeps whatever is already present in .env.

ARG_LOGIN=""
ARG_PASSWORD=""
ARG_ADMIN_ID=""

usage() {
    cat <<EOF
Usage: $(basename "$0") [options]

  -l, --login <value>       ScheduleClient login    -> ScheduleClient__login
  -p, --password <value>    ScheduleClient password -> ScheduleClient__password
  -a, --admin-id <value>    Telegram admin id       -> Admin__AdminId
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
        -h|--help)
            usage; exit 0 ;;
        *)
            log_err "unknown argument: $1"
            usage
            exit 1 ;;
    esac
done

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

# Copy .env into DiskayBot.Application
ENV_SRC="$SCRIPT_DIR/.env"
ENV_DST="$SCRIPT_DIR/DiskayBot/DiskayBot.Application/.env"
if [ -f "$ENV_SRC" ]; then
    cp "$ENV_SRC" "$ENV_DST"
    log_ok ".env copied to DiskayBot/DiskayBot.Application/"
else
    log_err ".env not found in $SCRIPT_DIR — aborting"
    exit 1
fi

# Override entries in the deployed .env with values passed as CLI arguments
upsert_env() {
    local key="$1"
    local value="$2"
    local file="$3"

    # remove any existing definition (tolerate spaces around '=')
    sed -i -E "/^[[:space:]]*${key}[[:space:]]*=/d" "$file"

    # ensure the file ends with a newline before appending
    if [ -s "$file" ] && [ -n "$(tail -c1 "$file")" ]; then
        printf '\n' >> "$file"
    fi

    printf '%s=%s\n' "$key" "$value" >> "$file"
}

if [ -n "$ARG_LOGIN" ]; then
    upsert_env "ScheduleClient__login" "$ARG_LOGIN" "$ENV_DST"
    log_ok "ScheduleClient__login set from argument"
fi

if [ -n "$ARG_PASSWORD" ]; then
    upsert_env "ScheduleClient__password" "$ARG_PASSWORD" "$ENV_DST"
    log_ok "ScheduleClient__password set from argument"
fi

if [ -n "$ARG_ADMIN_ID" ]; then
    upsert_env "Admin__AdminId" "$ARG_ADMIN_ID" "$ENV_DST"
    log_ok "Admin__AdminId set from argument ($ARG_ADMIN_ID)"
fi

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