# Adrians vevside

A personal ASP.NET Core website with small projects, experiments, football data, weather data and other things I want to host myself on my awesome RaspberyyPi

The project can run anyehere, can also be self-hosted with Docker, nginx, MariaDB and DuckDNS. Which I do!

## Site

- <https://vigdalpi.duckdns.org>

## Tech stack

- .NET 8
- ASP.NET Core MVC / Razor
- Entity Framework Core
- MariaDB
- Docker
- Docker Compose
- nginx
- Let’s Encrypt
- DuckDNS

## Repository structure

```text
Adrians/                 ASP.NET Core application
Adrians/Program.cs       Application startup/configuration
Adrians/appsettings.json Safe default configuration
docker-compose.yml       Docker services
Dockerfile               Standard Docker build
Dockerfile.pi            Raspberry Pi/runtime-only Docker build
nginx/                   nginx configuration
.env.example             Example environment variables
```

### Public repo

Safe defaults live in:

```text
Adrians/appsettings.json
```

This file should not contain real secrets, API keys, database passwords or machine-specific values.

### Windows development

Use .NET user secrets for local development secrets.

From the project folder:

```powershell
cd .\Adrians
dotnet user-secrets init
```

Add local secrets:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=xx;User=xx;Password=YourMomaXX;TreatTinyAsBoolean=false"
dotnet user-secrets set "Frost:ClientId" "<your-frost-client-id>"
dotnet user-secrets set "Frost:ClientSecret" "<your-frost-client-secret>"
dotnet user-secrets set "FootballData:ApiKey" "<your-football-data-api-key>"
```

Check configured secrets:

```powershell
dotnet user-secrets list
```

Do not commit user secrets.

### Docker / Raspberry Pi

Docker Compose reads variables from a local `.env` file in the repository root.

Create one from the template:

```bash
cp .env.example .env
```

Example:

```env
MARIADB_DATABASE=adriansdb
MARIADB_USER=adrian
MARIADB_PASSWORD=change-me
MARIADB_ROOT_PASSWORD=change-me

FROST_CLIENT_ID=change-me
FROST_CLIENT_SECRET=change-me
FOOTBALLDATA_API_KEY=change-me
```

Do not commit `.env`.

## Local development on Windows

Recommended setup:

- Visual Studio
- .NET SDK
- Docker Desktop
- Git

### 1. Clone the repository

```powershell
git clone git@github.com:Vigdals/AdriansVevside.git
cd AdriansVevside
```

### 2. Create local Docker environment file

```powershell
copy .env.example .env
```

Edit `.env` and set local database values.

For local development, simple throwaway values are fine:

```env
MARIADB_DATABASE=adriansdb
MARIADB_USER=adrian
MARIADB_PASSWORD=change-me
MARIADB_ROOT_PASSWORD=change-me
```

### 3. Start MariaDB

Start Docker Desktop first.

Then run from the repository root:

```powershell
docker compose up -d mariadb
```

Check status:

```powershell
docker compose ps
```

MariaDB should show as healthy.

Also check that Windows can reach the database:

```powershell
Test-NetConnection localhost -Port 3306
```

Expected:

```text
TcpTestSucceeded : True
```

### 4. Configure user secrets

From the ASP.NET project folder:

```powershell
cd .\Adrians
dotnet user-secrets init
```

Set the local connection string:

```powershell
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=3306;Database=adriansdb;User=adrian;Password=supersecret;TreatTinyAsBoolean=false"
```

Set API credentials as needed:

```powershell
dotnet user-secrets set "Frost:ClientId" "<your-frost-client-id>"
dotnet user-secrets set "Frost:ClientSecret" "<your-frost-client-secret>"
dotnet user-secrets set "FootballData:ApiKey" "<your-football-data-api-key>"
```

### 5. Run the app

From the repository root:

```powershell
dotnet restore .\Adrians.sln
dotnet build .\Adrians.sln
dotnet run --project .\Adrians\Adrians.csproj
```

Or run/debug the `Adrians` project directly from Visual Studio.

## Docker Compose

Start all services:

```bash
docker compose up -d
```

Start only MariaDB for local development:

```bash
docker compose up -d mariadb
```

Check services:

```bash
docker compose ps
```

View logs:

```bash
docker logs --tail=100 adriansvevside
docker logs --tail=100 nginx
docker logs --tail=100 mariadb
```

Stop services:

```bash
docker compose down
```

Reset local Docker database data:

```bash
docker compose down -v
docker compose up -d mariadb
```

## Raspberry Pi hosting

The site can be hosted on a Raspberry Pi using Docker Compose.

Basic request flow:

```text
Internet
  -> router
  -> nginx
  -> ASP.NET Core app
  -> MariaDB
```

Recommended public ports:

```text
80/tcp   HTTP
443/tcp  HTTPS
```

Only web traffic should be forwarded from the router. Do not expose admin tools such as Portainer, Dozzle, Glances or database ports to the internet.

## Raspberry Pi build

On a Raspberry Pi, the app can be published locally and then copied into a smaller runtime image.

From the repository root:

```bash
rm -rf publish
dotnet publish Adrians/Adrians.csproj -c Release -o publish /p:UseAppHost=false

docker build -f Dockerfile.pi -t adriansvevside-web .
docker compose up -d web nginx
```

Check the site:

```bash
curl -I https://vigdalpi.duckdns.org
```

Expected:

```text
HTTP/2 200
```

## Simple deployment flow

A simple self-hosted deployment flow is:

```text
Develop on Windows
  -> commit and push to GitHub
  -> Raspberry Pi fetches latest main
  -> publish .NET app
  -> build runtime Docker image
  -> restart web/nginx
  -> health check
```

Example deployment script on the Raspberry Pi:

```bash
#!/bin/sh
set -eu

APP_DIR="/home/pi/AdriansVevside"
HEALTH_URL="https://vigdalpi.duckdns.org"

cd "$APP_DIR"

echo "== Fetch latest code =="
git fetch origin main

LOCAL_SHA="$(git rev-parse HEAD)"
REMOTE_SHA="$(git rev-parse origin/main)"

if [ "$LOCAL_SHA" = "$REMOTE_SHA" ]; then
  echo "No changes: $(date -Is)"
  exit 0
fi

echo "== Deploying $LOCAL_SHA -> $REMOTE_SHA =="

git reset --hard origin/main

echo "== Validate compose =="
docker compose config >/dev/null

echo "== Publish .NET app =="
rm -rf publish
dotnet publish Adrians/Adrians.csproj -c Release -o publish /p:UseAppHost=false

echo "== Build runtime image =="
docker build -f Dockerfile.pi -t adriansvevside-web .

echo "== Start services =="
docker compose up -d web nginx

echo "== Health check =="
for i in $(seq 1 30); do
  if curl -fsSI "$HEALTH_URL" >/dev/null; then
    echo "Health OK"
    docker image prune -f >/dev/null
    echo "Deploy OK: $REMOTE_SHA $(date -Is)"
    exit 0
  fi

  sleep 2
done

echo "Health check failed"
docker compose ps
docker logs --tail=120 adriansvevside
docker logs --tail=80 nginx
exit 1
```

Make it executable:

```bash
chmod 700 ~/deploy-adriansvevside.sh
```

Run it manually:

```bash
~/deploy-adriansvevside.sh
```

Optional cron job:

```cron
*/5 * * * * /home/pi/deploy-adriansvevside.sh >> /home/pi/deploy-adriansvevside.log 2>&1
```

## GitHub access from the Raspberry Pi

For automatic pull/deploy, use a read-only deploy key without passphrase.

Recommended SSH alias:

```sshconfig
Host github-adriansvevside
    HostName github.com
    User git
    IdentityFile ~/.ssh/adriansvevside_deploy
    IdentitiesOnly yes
```

Set the repository remote:

```bash
git remote set-url origin github-adriansvevside:Vigdals/AdriansVevside.git
```

Test:

```bash
git fetch origin main
```

## Common issues

### Docker Desktop is not running

Error:

```text
failed to connect to the docker API
```

Fix: start Docker Desktop and retry.

### MariaDB variables are blank

Warnings like:

```text
The "MARIADB_PASSWORD" variable is not set
```

Fix: create `.env` in the repository root.

### App cannot connect to MySQL

Check that MariaDB is running and published on localhost:

```powershell
docker compose ps
Test-NetConnection localhost -Port 3306
```

The compose file should publish MariaDB like this for local development:

```yaml
ports:
  - "127.0.0.1:3306:3306"
```

### Missing API credentials

Use user secrets on Windows:

```powershell
dotnet user-secrets list
```

Expected names:

```text
ConnectionStrings:DefaultConnection
Frost:ClientId
Frost:ClientSecret
FootballData:ApiKey
```

Use `.env` / environment variables in Docker and on the Raspberry Pi.

## Security notes

- Do not commit `.env`.
- Do not commit API keys or passwords.
- Use user secrets for Windows development.
- Use environment variables for Docker/Pi deployments.
- Keep database and admin tools private.
- Only expose HTTP/HTTPS publicly.

## Maintainer

- [Adrian](https://github.com/vigdals)

## License

This project is licensed under the MIT License. See `LICENSE` for details.
