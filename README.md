<p align="center">
  <img src=".github/assets/banner.png" alt="Diskay">
</p>

# Diskay bot

Telegram bot that shows the [IT College](https://students.it-college.ru) class schedule for a student's group. Users and groups are stored by [DiskayMemory](https://github.com/DiskayHub/DiskayMemory); schedules are cached in Redis.

## Commands

| Command | Access | Description |
|---|---|---|
| `/start` | everyone | Welcome message |
| `/create_account` | everyone | Register and pick a course and group |
| `/disky` | user | Nearest schedule for your group, with day navigation |
| `/check` | user | Schedule of any group |
| `/settings` | user | Change group, toggle news notifications |
| `/show_profile` | user | Profile data |
| `/about` | everyone | About the bot and its version |
| `/check_bot_status` | everyone | DiskayMemory availability |
| `/create_news <text>` | admin | Broadcast to users with notifications enabled, after confirmation |

The admin is the Telegram user whose id is set in `Admin__AdminId`.

## How it works

A background worker fetches the current week for every group known to DiskayMemory every `ScheduleService:updateTimeout` seconds (120 by default) and stores each day in Redis for `Redis:scheduleExpireDays` days. Commands read schedules from Redis.

| Project | Responsibility |
|---|---|
| `DiskayBot.Application` | Entry point: Telegram commands and callbacks, schedule worker, DI |
| `DiskayBot.API` | HTTP clients for DiskayMemory and the college schedule portal |
| `DiskayBot.Redis` | Redis access (`IRedisController`) |
| `DiskayBot.Tests` | Tests |

## Configuration

Settings come from `DiskayBot.Application/appsettings.json`, then `appsettings.Docker.json` when running in Docker, then environment variables.

| Variable | Description |
|---|---|
| `TelegramBot__Token` | Bot API token |
| `Admin__AdminId` | Telegram id of the admin |
| `ScheduleClient__login`, `ScheduleClient__password` | College schedule portal credentials |
| `Redis__ConnectionString` | e.g. `localhost:6379,password=<pass>,abortConnect=false` |
| `UserClient__url` | DiskayMemory URL, `http://localhost:8080` by default |

## Running locally

Requires the [.NET 9 SDK](https://dotnet.microsoft.com/download), a running Redis and DiskayMemory.

1. Put the variables above into `DiskayBot.Application/.env`.
2. Build and start the bot from the output directory — `.env` and `appsettings.json` are resolved three levels up from the working directory, so `dotnet run` from the project folder does not pick up `.env`:

```bash
cd DiskayBot.Application
dotnet build
cd bin/Debug/net9.0
dotnet DiskayBot.Application.dll
```

In an IDE, set the working directory of the run configuration to `bin/Debug/net9.0`.

## Deployment

Every push to `master` runs [`.github/workflows/deploy.yml`](.github/workflows/deploy.yml): it connects to the server over SSH, clones or updates the repository in `~/projects/DiskayHub/DiskayBot` and runs [`deploy.sh`](deploy.sh). The script clones DiskayMemory, generates the bot `.env` from the secrets, backs up Postgres, starts the stack from [`docker-compose.yml`](docker-compose.yml) and checks the services.

**Repository secrets**

| Secret | Description |
|---|---|
| `SSH_HOST`, `SSH_USER`, `SSH_PRIVATE_KEY` | SSH access to the server; the public key must be in the server's `authorized_keys` |
| `BOT_TOKEN`, `ADMIN_ID` | Telegram bot token and admin id |
| `SCHEDULE_LOGIN`, `SCHEDULE_PASSWORD` | College schedule portal credentials |
| `REDIS_PASSWORD` | Redis password |
| `POSTGRES_PASSWORD` | Postgres password; applied to the existing database on every deploy |

`REDIS_PASSWORD` and `POSTGRES_PASSWORD` must contain only letters and digits — `deploy.sh` rejects anything else.

**Server requirements**

- Docker with the Compose plugin.
- An SSH key with read access to `DiskayHub/DiskayBot` and `DiskayHub/DiskayMemory` on GitHub.
- Access to `students.it-college.ru` — it is not reachable from every country.

The stack runs as the `diskayhub` Compose project from `~/projects/DiskayHub`; no ports are published to the host. Passwords are passed only by `deploy.sh`, so a manual `docker compose up` needs `REDIS_PASSWORD` and `POSTGRES_PASSWORD` exported first.
