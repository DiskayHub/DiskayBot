#!/bin/bash
set -euo pipefail

# Moves DiskayHub from one server to another. Runs on a local machine that has
# key-based SSH access to both servers; the servers never talk to each other.
#
#   1. checks both servers and reads the current secrets from the old one
#   2. installs git and Docker on the new server if they are missing
#   3. generates the CD key (GitHub Actions -> new server) and authorizes it
#   4. gives the new server its own GitHub key for cloning both repositories
#   5. stops the bot on the old server and dumps the database   <- downtime starts
#   6. deploys to the new server, restoring the dump before DiskayMemory starts
#   7. compares row counts of every table on both servers
#   8. prints the values for the GitHub Actions secrets
#
# The old server's postgres and redis are left running and its data untouched.

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

log_step() { echo -e "\n${BLUE}${BOLD}▶ $1${RESET}"; }
log_ok()   { echo -e "  ${GREEN}✔ $1${RESET}"; }
log_warn() { echo -e "  ${YELLOW}⚠ $1${RESET}"; }
log_err()  { echo -e "  ${RED}✘ $1${RESET}"; }
log_info() { echo -e "  ${CYAN}→ $1${RESET}"; }
die()      { log_err "$1"; exit 1; }

# Same layout as the deploy workflow uses
HUB_DIR_REL="projects/DiskayHub"
REPOS=("git@github.com:DiskayHub/DiskayBot.git" "git@github.com:DiskayHub/DiskayMemory.git")
GITHUB_KEY_NAME="diskayhub_github"

usage() {
    cat <<EOF
Usage: $(basename "$0") --old <ssh target> --new <ssh target> [--out <dir>]

  --old <target>   current server, e.g. user@1.2.3.4 or an alias from ~/.ssh/config
  --new <target>   server to move to
  --out <dir>      where to keep the dump, the CD key and the secrets
                   (default: ~/diskayhub-migration-<timestamp>, outside the repository)
  -h, --help       show this help

Both targets must accept SSH keys without a password prompt.
EOF
}

OLD=""
NEW=""
OUT_DIR="$HOME/diskayhub-migration-$(date +%Y%m%d_%H%M%S)"

while [ $# -gt 0 ]; do
    case "$1" in
        --old) [ $# -ge 2 ] || die "$1 requires a value"; OLD="$2"; shift 2 ;;
        --new) [ $# -ge 2 ] || die "$1 requires a value"; NEW="$2"; shift 2 ;;
        --out) [ $# -ge 2 ] || die "$1 requires a value"; OUT_DIR="$2"; shift 2 ;;
        -h|--help) usage; exit 0 ;;
        *) log_err "unknown argument: $1"; usage; exit 1 ;;
    esac
done

[ -n "$OLD" ] && [ -n "$NEW" ] || { usage; exit 1; }

# stdin is closed so ssh does not eat answers typed for the prompts below
ssh_old() { ssh -o BatchMode=yes "$OLD" "$@" </dev/null; }
ssh_new() { ssh -o BatchMode=yes "$NEW" "$@" </dev/null; }

confirm() {
    local answer
    read -r -p "  $1 [y/N] " answer
    [[ "$answer" =~ ^[YyДд]$ ]]
}

# Letters and digits only: deploy.sh rejects anything else for these passwords
gen_password() {
    local raw
    raw=$(head -c 64 /dev/urandom | base64 | tr -dc 'A-Za-z0-9')
    printf '%s' "${raw:0:32}"
}

# One count per table, so the restore can be checked table by table.
# The SQL goes through stdin: inside an ssh command line its quotes would need escaping
COUNT_SQL="SELECT format('SELECT %L, count(*) FROM %I.%I', schemaname || '.' || tablename, schemaname, tablename)
           FROM pg_tables WHERE schemaname NOT IN ('pg_catalog', 'information_schema') ORDER BY 1 \\gexec"

count_rows() {
    printf '%s\n' "$COUNT_SQL" \
        | ssh -o BatchMode=yes "$1" "docker exec -i diskay_postgres psql -U postgres -d diskay_memory -At"
}

rollback_hint() {
    log_info "the old server's data is untouched; to switch back:"
    log_info "  ssh $NEW 'docker stop diskayBot'"
    log_info "  ssh $OLD 'docker start diskayMemory diskayBot'"
}

ssh_option() {
    ssh -G "$1" | awk -v key="$2" '$1 == key { print $2; exit }'
}

echo -e "\n${BOLD}════════════════════════════════════════${RESET}"
echo -e "${BOLD}        DiskayHub Server Migration       ${RESET}"
echo -e "${BOLD}════════════════════════════════════════${RESET}"

# ─────────────────────────────────────────────
#  Step 1: Check servers
# ─────────────────────────────────────────────
log_step "Step 1: Checking servers"

for tool in ssh ssh-keygen scp; do
    command -v "$tool" >/dev/null || die "$tool is not installed locally"
done

NEW_HOST=$(ssh_option "$NEW" hostname)
NEW_USER=$(ssh_option "$NEW" user)
NEW_PORT=$(ssh_option "$NEW" port)
OLD_HOST=$(ssh_option "$OLD" hostname)
[ "$NEW_HOST" != "$OLD_HOST" ] || die "--old and --new point to the same host: $NEW_HOST"

ssh_old true || die "cannot connect to the old server ($OLD)"
log_ok "old server reachable: $OLD"
ssh_new true || die "cannot connect to the new server ($NEW)"
log_ok "new server reachable: $NEW ($NEW_USER@$NEW_HOST:$NEW_PORT)"

ssh_old "test -f ~/$HUB_DIR_REL/docker-compose.yml" \
    || die "old server has no ~/$HUB_DIR_REL/docker-compose.yml — is it really the current server?"
ssh_old "docker inspect diskay_postgres >/dev/null 2>&1" \
    || die "old server has no diskay_postgres container"
log_ok "old server has the DiskayHub stack"

NEW_HOME=$(ssh_new 'printf %s "$HOME"')
NEW_HUB_DIR="$NEW_HOME/$HUB_DIR_REL"

if ssh_new "docker inspect diskay_postgres >/dev/null 2>&1"; then
    log_warn "the new server already has a diskay_postgres container"
    log_warn "its diskay_memory database will be REPLACED by the old server's one (deploy.sh backs it up first)"
    confirm "Continue?" || die "aborted"
fi

mkdir -p "$OUT_DIR"
chmod 700 "$OUT_DIR"
log_ok "output directory: $OUT_DIR"

# ─────────────────────────────────────────────
#  Step 2: Read secrets from the old server
# ─────────────────────────────────────────────
log_step "Step 2: Reading secrets from the old server"

OLD_ENV=$(ssh_old "cat ~/$HUB_DIR_REL/DiskayBot/DiskayBot.Application/.env 2>/dev/null" | tr -d '\r' || true)
OLD_DB_CONNECTION=$(ssh_old "docker inspect -f '{{range .Config.Env}}{{println .}}{{end}}' diskayMemory 2>/dev/null" \
    | sed -n 's/^ConnectionStrings__DiskayDbContext=//p' || true)

env_value() {
    printf '%s\n' "$OLD_ENV" | sed -nE "s/^[[:space:]]*$1[[:space:]]*=[[:space:]]*//p" | tail -n 1
}

# Values the script cannot make up are asked for; passwords of the fresh
# redis/postgres can simply be new, deploy.sh applies them either way
declare -A SECRET
declare -A ORIGIN

take_secret() {
    local name="$1" value="$2" hidden="$3"
    if [ -n "$value" ]; then
        SECRET[$name]="$value"
        ORIGIN[$name]="as on the old server"
        log_ok "$name — read from the old server"
        return
    fi
    log_warn "$name — not found on the old server"
    if [ "$hidden" = "hidden" ]; then
        read -r -s -p "  Enter $name: " value; echo
    else
        read -r -p "  Enter $name: " value
    fi
    [ -n "$value" ] || die "$name is required"
    SECRET[$name]="$value"
    ORIGIN[$name]="entered manually — UPDATE the GitHub secret"
}

take_password() {
    local name="$1" value="$2"
    if [[ "$value" =~ ^[A-Za-z0-9]+$ ]]; then
        SECRET[$name]="$value"
        ORIGIN[$name]="as on the old server"
        log_ok "$name — read from the old server"
    else
        SECRET[$name]=$(gen_password)
        ORIGIN[$name]="NEW — UPDATE the GitHub secret"
        log_warn "$name — not found on the old server, generated a new one"
    fi
}

take_secret   SCHEDULE_LOGIN    "$(env_value ScheduleClient__login)"    visible
take_secret   SCHEDULE_PASSWORD "$(env_value ScheduleClient__password)" hidden
take_secret   ADMIN_ID          "$(env_value Admin__AdminId)"           visible
take_secret   BOT_TOKEN         "$(env_value TelegramBot__Token)"       hidden
take_password REDIS_PASSWORD    "$(env_value Redis__ConnectionString | sed -nE 's/.*password=([^,]*).*/\1/p')"
take_password POSTGRES_PASSWORD "$(printf '%s' "$OLD_DB_CONNECTION" | sed -nE 's/.*Password=([^;]*).*/\1/p')"

# ─────────────────────────────────────────────
#  Step 3: Prepare the new server
# ─────────────────────────────────────────────
log_step "Step 3: Preparing the new server"

MISSING_TOOLS=$(ssh_new 'for t in git curl docker; do command -v $t >/dev/null || echo $t; done
                         command -v docker >/dev/null && ! docker compose version >/dev/null 2>&1 && echo compose
                         true')

if [ -n "$MISSING_TOOLS" ]; then
    log_info "missing on the new server: $(echo $MISSING_TOOLS)"
    ssh_new 'command -v apt-get >/dev/null' \
        || die "automatic install supports only Debian/Ubuntu (apt-get); install git and Docker manually"
    log_info "installing (sudo may ask for the password)..."
    # -t: sudo needs a terminal for the password prompt
    ssh -t "$NEW" 'set -e
        SUDO=; [ "$(id -u)" -eq 0 ] || SUDO=sudo
        $SUDO apt-get update -q
        $SUDO apt-get install -y -q git curl ca-certificates
        if ! command -v docker >/dev/null || ! docker compose version >/dev/null 2>&1; then
            curl -fsSL https://get.docker.com | $SUDO sh
        fi
        [ "$(id -u)" -eq 0 ] || $SUDO usermod -aG docker "$USER"'
    log_ok "git and Docker installed"
else
    log_ok "git, curl, docker and docker compose are present"
fi

# A new SSH session picks up the docker group added above
ssh_new 'docker info >/dev/null 2>&1' \
    || die "user $NEW_USER cannot use docker on the new server (not in the docker group or the daemon is down)"
log_ok "$NEW_USER can use docker"

# ─────────────────────────────────────────────
#  Step 4: CD key (GitHub Actions -> new server)
# ─────────────────────────────────────────────
log_step "Step 4: Generating the CD key"

CD_KEY="$OUT_DIR/cd_deploy_key"
ssh-keygen -q -t ed25519 -N '' -C "diskayhub-cd@$NEW_HOST" -f "$CD_KEY"
CD_PUB=$(cat "$CD_KEY.pub")

ssh_new "mkdir -p ~/.ssh && chmod 700 ~/.ssh && touch ~/.ssh/authorized_keys && chmod 600 ~/.ssh/authorized_keys
         grep -qxF '$CD_PUB' ~/.ssh/authorized_keys || echo '$CD_PUB' >> ~/.ssh/authorized_keys"

ssh -i "$CD_KEY" -o IdentitiesOnly=yes -o BatchMode=yes -p "$NEW_PORT" "$NEW_USER@$NEW_HOST" true </dev/null \
    || die "the new server does not accept the generated CD key"
log_ok "CD key generated and authorized: $CD_KEY"

# ─────────────────────────────────────────────
#  Step 5: GitHub access for the new server
# ─────────────────────────────────────────────
log_step "Step 5: Checking GitHub access from the new server"

github_access_ok() {
    local repo
    for repo in "${REPOS[@]}"; do
        ssh_new "GIT_SSH_COMMAND='ssh -o StrictHostKeyChecking=accept-new -o BatchMode=yes' git ls-remote '$repo' HEAD >/dev/null 2>&1" \
            || return 1
    done
}

if github_access_ok; then
    log_ok "the new server can already read both repositories"
else
    ssh_new "test -f ~/.ssh/$GITHUB_KEY_NAME || ssh-keygen -q -t ed25519 -N '' -C 'diskayhub-server@$NEW_HOST' -f ~/.ssh/$GITHUB_KEY_NAME
             touch ~/.ssh/config && chmod 600 ~/.ssh/config
             grep -q '$GITHUB_KEY_NAME' ~/.ssh/config || printf '\nHost github.com\n    IdentityFile ~/.ssh/$GITHUB_KEY_NAME\n' >> ~/.ssh/config"
    GITHUB_PUB=$(ssh_new "cat ~/.ssh/$GITHUB_KEY_NAME.pub")

    echo ""
    log_info "Add this public key to GitHub so the new server can clone both repositories:"
    echo ""
    echo "      $GITHUB_PUB"
    echo ""
    log_info "GitHub → Settings → SSH and GPG keys → New SSH key, on an account that can read"
    log_info "both DiskayHub/DiskayBot and DiskayHub/DiskayMemory."
    log_warn "A repository deploy key will NOT work: it opens only one repository."

    until github_access_ok; do
        echo ""
        read -r -p "  Press Enter once the key is added (q to abort): " answer
        [ "$answer" != "q" ] || die "aborted"
    done
    log_ok "the new server can read both repositories"
fi

# ─────────────────────────────────────────────
#  Step 6: Stop the old bot and dump the database
# ─────────────────────────────────────────────
log_step "Step 6: Stopping the old server and dumping the database"

log_warn "The bot goes offline now and stays offline until the new server is up."
log_warn "Two bots with the same token conflict, so the old one has to stop first."
confirm "Stop the bot on $OLD?" || die "aborted, nothing has been changed on the servers"

# DiskayMemory is stopped too, so nothing writes to the database during the dump.
# restart: unless-stopped keeps manually stopped containers down after a reboot
ssh_old "docker stop diskayBot diskayMemory >/dev/null 2>&1 || true"
log_ok "diskayBot and diskayMemory stopped on the old server"

DUMP_NAME="migration_$(date +%Y%m%d_%H%M%S).sql"
LOCAL_DUMP="$OUT_DIR/$DUMP_NAME"
ssh_old "docker exec diskay_postgres pg_dumpall -U postgres" > "$LOCAL_DUMP"
grep -q 'PostgreSQL database cluster dump complete' "$LOCAL_DUMP" \
    || die "the dump looks incomplete: $LOCAL_DUMP"
chmod 600 "$LOCAL_DUMP"
log_ok "dump saved: $LOCAL_DUMP ($(du -h "$LOCAL_DUMP" | cut -f1))"

OLD_COUNTS=$(count_rows "$OLD")

# ─────────────────────────────────────────────
#  Step 7: Deploy to the new server
# ─────────────────────────────────────────────
log_step "Step 7: Deploying to the new server"

ssh_new "mkdir -p '$NEW_HUB_DIR/backup'"
scp -q "$LOCAL_DUMP" "$NEW:$NEW_HUB_DIR/backup/$DUMP_NAME"
log_ok "dump uploaded to $NEW_HUB_DIR/backup/$DUMP_NAME"

# The local deploy.sh is used, not the one from GitHub: it has to match this script.
# deploy.sh itself clones both repositories and syncs them with origin/master
scp -q "$SCRIPT_DIR/deploy.sh" "$NEW:$NEW_HUB_DIR/deploy.sh"

DEPLOY_ARGS=$(printf '%q ' \
    --login "${SECRET[SCHEDULE_LOGIN]}" \
    --password "${SECRET[SCHEDULE_PASSWORD]}" \
    --admin-id "${SECRET[ADMIN_ID]}" \
    --token "${SECRET[BOT_TOKEN]}" \
    --redis-password "${SECRET[REDIS_PASSWORD]}" \
    --db-password "${SECRET[POSTGRES_PASSWORD]}" \
    --restore "$NEW_HUB_DIR/backup/$DUMP_NAME")

if ! ssh_new "bash '$NEW_HUB_DIR/deploy.sh' $DEPLOY_ARGS"; then
    log_err "deploy on the new server failed, see the output above"
    rollback_hint
    exit 1
fi

# ─────────────────────────────────────────────
#  Step 8: Verify
# ─────────────────────────────────────────────
log_step "Step 8: Verifying the migration"

NEW_COUNTS=$(count_rows "$NEW")

if [ "$OLD_COUNTS" = "$NEW_COUNTS" ]; then
    while IFS='|' read -r table count; do
        log_ok "$table — $count rows"
    done <<< "$NEW_COUNTS"
else
    log_err "row counts differ (old vs new):"
    diff <(echo "$OLD_COUNTS") <(echo "$NEW_COUNTS") || true
    rollback_hint
    die "check the database on the new server before switching GitHub to it"
fi

BOT_STARTED=""
for i in $(seq 1 15); do
    if ssh_new "docker logs diskayBot 2>&1 | grep -q 'запущен'"; then
        BOT_STARTED=1
        break
    fi
    sleep 2
done
if [ -n "$BOT_STARTED" ]; then
    log_ok "bot connected to Telegram"
else
    log_warn "bot has not reported a Telegram connection in 30s — check: ssh $NEW 'docker logs diskayBot'"
fi

# ─────────────────────────────────────────────
#  Step 9: GitHub Actions secrets
# ─────────────────────────────────────────────
SECRETS_FILE="$OUT_DIR/github-secrets.txt"
{
    echo "GitHub → DiskayHub/DiskayBot → Settings → Secrets and variables → Actions"
    echo ""
    echo "SSH_HOST          = $NEW_HOST                      (NEW — UPDATE)"
    echo "SSH_USER          = $NEW_USER                      (NEW — UPDATE)"
    echo "SSH_PRIVATE_KEY   = whole file $CD_KEY, including the BEGIN/END lines (NEW — UPDATE):"
    cat "$CD_KEY"
    for name in SCHEDULE_LOGIN SCHEDULE_PASSWORD ADMIN_ID BOT_TOKEN REDIS_PASSWORD POSTGRES_PASSWORD; do
        printf '%-17s = %s      (%s)\n' "$name" "${SECRET[$name]}" "${ORIGIN[$name]}"
    done
} > "$SECRETS_FILE"
chmod 600 "$SECRETS_FILE"

log_step "Step 9: GitHub Actions secrets"
echo ""
cat "$SECRETS_FILE"
echo ""

if [ "$NEW_PORT" != "22" ]; then
    log_warn "the new server listens on SSH port $NEW_PORT, but the deploy workflow always connects to 22"
fi

echo -e "${GREEN}${BOLD}════════════════════════════════════════${RESET}"
echo -e "${GREEN}${BOLD}  The bot is running on the new server  ${RESET}"
echo -e "${GREEN}${BOLD}════════════════════════════════════════${RESET}"
echo ""
log_warn "Update the SSH_* secrets BEFORE the next push to master: until then the workflow"
log_warn "deploys to the old server and starts the second bot there."
log_info "Everything is saved in $OUT_DIR (dump, CD key, secrets) — keep it private."
log_info "The old server's postgres and redis are still running with the data intact."
log_info "Once the new server is confirmed, shut the old one down:"
log_info "  ssh $OLD 'cd ~/$HUB_DIR_REL && docker compose down'"
