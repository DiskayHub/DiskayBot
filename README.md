# Diskay bot

## How to build

### 1. Install Dotnet

#### Arch Linux

```bash
sudo pacman -S dotnet-sdk
```

#### Ubuntu

```bash
sudo apt-get install -y dotnet-sdk-9.0
```

### 2. Setting up a bot

Create `.env` file in **DiskayBot.Bot**, and put your bot token:

```env
BOT_TOKEN="YOUR_BOT_TOKEN"
```

### 3. Configure the configuration

In file `appsettings.json`, you need configurate the services:

```json
    "Configuration": {
        "Services": {
            "DiskayMemory": {
                "url": "http://localhost:8080" // Put your service url
            },
            "Redis": {
                "url": "localhost:6379" // Put your service url
            }
        }
    }
```

### 4. Run project

In the bot folder (`DiskayBot.Bot`) 

```bash
dotnet run
```