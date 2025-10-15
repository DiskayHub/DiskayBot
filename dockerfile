# ===== Билд-стейдж =====
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Скопировать sln и все csproj
COPY DiskayBot.sln ./
COPY DiskayBot.API/DiskayBot.API.csproj DiskayBot.API/
COPY DiskayBot.Bot/DiskayBot.Bot.csproj DiskayBot.Bot/
COPY DiskayBot.Redis/DiskayBot.Redis.csproj DiskayBot.Redis/
COPY DiskayBot.Services/DiskayBot.Services.csproj DiskayBot.Services/
COPY DiskayBot.Tests/DiskayBot.Tests.csproj DiskayBot.Tests/

# Восстановление зависимостей
RUN dotnet restore

# Скопировать всё остальное и собрать
COPY . .
COPY DiskayBot.Bot/appsettings.json ./
RUN dotnet publish DiskayBot.Bot/DiskayBot.Bot.csproj -c Release -o /app/publish

# ===== Runtime-стейдж =====
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Установка переменной окружения для временной зоны
ENV TZ=Asia/Yekaterinburg

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "DiskayBot.Bot.dll"]