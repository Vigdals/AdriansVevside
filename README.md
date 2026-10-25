# AdriansVevside

Personleg nettside og dashboard utvikla i **ASP.NET Core MVC på .NET 10**, med vêr, fotball, informasjon frå Raspberry Pi og ei nyheitsoversikt for utvikling og informasjonssikkerheit. Prosjektet er eit hobbyprosjekt og blir drifta med Docker, MariaDB og Nginx på Raspberry Pi.

**Nettside:** https://vigdalpi.duckdns.org  
**Kjeldekode:** https://github.com/Vigdals/AdriansVevside  
**Lisens:** [MIT](LICENSE)

> Denne README-en dokumenterer koden i `main` per oktober 2026. Han beskriv ikkje nødvendigvis alle manuelle endringar på den faktiske serveren. Føresetnader for Nginx, sertifikat, ekstern overvaking og deploy må kontrollerast på verten.

## Funksjonar og ruter

| Rute | Innhald | Datakjelde |
| --- | --- | --- |
| `/` | Dashboard med klokke, vêr, neste Sogndal-/Barcelona-kamp, tømmekalender, nedteljingar og Pi-/tenestestatus | met.no, NIFS, SIMAS/Norkart, Uptime Kuma og lokal deploy-status |
| `/Home/Pi` | Forenkla informasjonsskjerm med klokke, vêr og tømmekalender; automatisk oppfrisking kvart femte minutt | Som over |
| `/Digi` | Samla nyheiter om sikkerheit og utvikling, med søk og filter for kjelde, kategori og tidsrom | RSS/Atom, sjå nedanfor |
| `/HackerNews` | Artiklar frå Hacker News | Hacker News API |
| `/Fpl` | Komande fristar i Fantasy Premier League | FPL API |
| `/Barca` | Komande Barcelona-kampar | football-data.org |
| `/Langrenn` | Snødjupn og korttidsvêr for utvalde stader | MET Frost og met.no |
| `/Langrenn/Heggis`, `/Langrenn/Hodlekve`, `/Langrenn/Brunestegen` | Detaljvisingar for vêr og eventuelt snødjupn | MET Frost og met.no |
| `/KampInfo?stageId=...` | Kampar i ein vald turneringsfase | NIFS |
| `/RPG` og `/Game` | Eldre, eksperimentell rollespeldel | EF Core / MariaDB |

Den eksperimentelle rollespeldelen brukar `[Authorize]`, men `Program.cs` set ikkje opp ei komplett autentiseringsløysing. Ikkje rekn med at desse endepunkta er operative utan meir arbeid.

### Nyheitsoversikta på /Digi

`DigiController` hentar og slår saman nyheiter frå:

- **NSM**: sårbarheitsvarsel
- **CISA**: sikkerheitsvarsel
- **BleepingComputer**: sikkerheitsnyheiter
- **Microsoft MSRC**: Microsoft-sikkerheitsnyheiter
- **Krebs on Security**: sikkerheitsanalysar
- **GitHub Security**: sikkerheit i utvikling og leveransekjeder
- **.NET Blog**: .NET og programvareutvikling

Artiklane blir sorterte etter publiseringstidspunkt (nyaste først) og avgrensa til 180 samla treff. Sida støttar søk og filtrering i nettlesaren utan nye serverkall. Kvar feed blir henta uavhengig, med om lag åtte sekund timeout, maksimum 35 artiklar per kjelde og ti minutt minnecache. Ved feil blir sist vellukka resultat brukt i inntil 12 timar, dersom det finst i minnet. Kjelder kan endre feed-adresser eller blokkere automatisk innhenting.

Feed-lista er hardkoda i `Adrians/Controllers/DigiController.cs`. For å endre kjelder, endrar du lista der. Den eldre `RssFeedService` er ei separat teneste og er ikkje grunnlaget for den nye Digi-oversikta.

## Teknologi

- **.NET 10 / ASP.NET Core MVC / Razor**
- **Entity Framework Core 9** med **Pomelo.EntityFrameworkCore.MySql 9**
- **MariaDB 11.4** (speldata, EF Core-modellar og migrasjonar)
- **Bootstrap 5** og eigen CSS/JavaScript
- **IHttpClientFactory**, `IMemoryCache` og eksterne REST-/RSS-tenester
- **Docker / Docker Compose**, `Dockerfile` og `Dockerfile.pi`
- **Nginx**, HTTPS-sertifikat frå Let's Encrypt og DNS handtert utanfor applikasjonen
- **GitHub Actions**, Dependabot og CodeQL

Merk at rammeverket er .NET 10, medan fleire NuGet-pakkar framleis ligg på EF Core 9 eller eldre versjonar. Oppgraderingar må testast separat; README-en føreset ikkje at alle pakkar har same hovudversjon.

## Prosjektstruktur

~~~text
Adrians.sln
Adrians/
  Adrians.csproj
  Program.cs                  Oppstart, DI, HTTP-klientar, cache, databasen og ruting
  Controllers/                MVC-endepunkt og Digi sin RSS-aggregator
  Services/                   Vêr, fotball, tømmekalender, Uptime Kuma, Pi-status m.m.
  Resources/                  API-klientar og konfigurasjonsmodellar
  Data/                       GameContext og design-time DbContext-fabrikk
  Migrations/                 EF Core-migrasjonar for rollespeldatabasen
  Models/                     Domene- og API-modellar
  ViewModels/                 Visingsmodellar
  Views/                      Razor-visingar og felles layout
  wwwroot/                    CSS, JS, bilete, vêr-ikon og offentlege statusfiler
  appsettings.json            Ikkje-sensitive standardverdiar
  nuget.config                NuGet-kjelder
Dockerfile                    Bygg og publish med .NET 10 SDK i Docker
Dockerfile.pi                 Berre runtime; krev lokal publish/-mappe
docker-compose.yml            MariaDB, web og nginx
nginx/app.conf                Døme på proxy-konfigurasjon (ikkje automatisk montert)
.env.example                  Utgangspunkt for lokal Docker-konfigurasjon
.github/workflows/            CodeQL, ZAP og manuell tryggleiksskann
~~~

## Føresetnader

For lokal utvikling:

- .NET **10 SDK**
- Git
- Docker med Docker Compose, til dømes Docker Desktop
- Ei tilgjengeleg MariaDB-instans dersom du skal bruke databaseavhengige funksjonar
- Eventuelt Visual Studio eller VS Code

For Raspberry Pi-drift treng du òg Docker Engine/Compose, .NET 10 SDK på verten dersom du brukar `Dockerfile.pi`-løpet, ein fungerande reverse proxy og DNS/TLS-oppsett. Docker-imaga for .NET 10 må støtte arkitekturen på verten.

## Lokal utvikling (Windows / PowerShell)

### 1. Klon og start databasen

~~~powershell
git clone https://github.com/Vigdals/AdriansVevside.git
cd AdriansVevside

Copy-Item .env.example .env
# Rediger .env og set unike, lokale databasepassord.

docker compose up -d mariadb
docker compose ps
Test-NetConnection 127.0.0.1 -Port 3306
~~~

`docker-compose.yml` bind MariaDB til `127.0.0.1:3306` på verten, ikkje til alle nettverksgrensesnitt. Compose-fila refererer òg til `FROST_CLIENT_ID`, `FROST_CLIENT_SECRET` og `FOOTBALLDATA_API_KEY`. Desse manglar i `.env.example`. For å unngå varsel frå Compose kan du leggje dei inn i lokal `.env` med eigne verdiar, eller tomme verdiar når funksjonane ikkje skal brukast.

### 2. Konfigurer utviklingshemmelegheiter

Prosjektet har allereie ein `UserSecretsId` i `Adrians.csproj`. Du treng ikkje køyre `dotnet user-secrets init` på nytt.

~~~powershell
cd Adrians

dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=127.0.0.1;Port=3306;Database=adriansdb;User=adrian;Password=<lokalt-passord>;TreatTinyAsBoolean=false"
dotnet user-secrets set "Frost:ClientId" "<din-klient-id>"
dotnet user-secrets set "Frost:ClientSecret" "<din-klienthemmelegheit>"
dotnet user-secrets set "FootballData:ApiKey" "<din-api-nokkel>"

cd ..
~~~

Databasebrukar, passord og databasenamn må samsvare med verdiane i `.env`. Frost/football-data-nøklar er berre nødvendige for dei tilhøyrande funksjonane. Ikkje legg reelle nøklar eller passord i Git, `appsettings.json` eller dokumentasjonen.

### 3. Bygg og start applikasjonen

~~~powershell
dotnet restore .\Adrians.sln
dotnet build .\Adrians.sln -c Debug
dotnet run --project .\Adrians\Adrians.csproj
~~~

Utviklingsprofilen i `Adrians/Properties/launchSettings.json` brukar:

- https://localhost:7289
- http://localhost:5063

Godta eller opprett lokalt HTTPS-utviklarsertifikat ved behov (`dotnet dev-certs https --trust`).

**Merk:** `Adrians/nuget.config` har både nuget.org og ei ekstra GitHub Packages-kjelde. Restore kan krevje tilgang og legitimasjon til denne kjelda sjølv om du berre ønskjer å byggje dei offentlege avhengigheitene. Handter dette lokalt, ikkje ved å commite tilgangstoken.

### 4. Databasemigrasjonar (berre ved behov)

EF Core-modellane og migrasjonane høyrer til rollespeldelen. Applikasjonen køyrer ikkje migrasjonar automatisk ved oppstart.

Design-time-fabrikken i `Adrians/Data/GameContextFactory.cs` les miljøvariabelen `ConnectionStrings__DefaultConnection`, ikkje .NET User Secrets. Set variabelen eksplisitt når du køyrer migrasjonar:

~~~powershell
$env:ConnectionStrings__DefaultConnection = "Server=127.0.0.1;Port=3306;Database=adriansdb;User=adrian;Password=<lokalt-passord>;TreatTinyAsBoolean=false"
dotnet tool restore --tool-manifest .\Adrians\.config\dotnet-tools.json
dotnet ef database update --project .\Adrians\Adrians.csproj
Remove-Item Env:\ConnectionStrings__DefaultConnection
~~~

Kontroller kva migrasjonane gjer, og ta backup før du køyrer dei mot ein database med data. `GameContextFactory` nyttar `ServerVersion.AutoDetect` og krev difor tilgang til MariaDB under migrasjon.

## Docker og Raspberry Pi

Repoet har **to ulike Dockerfile-ar**:

| Fil | Bruk | Føresetnad |
| --- | --- | --- |
| `Dockerfile` | Multi-stage: byggjer og publiserer med SDK inne i Docker | Kan byggjast frå repo-rota |
| `Dockerfile.pi` | Runtime-image med helsesjekk | `publish/` må finnast i repo-rota før bygging |

**Viktig:** `docker-compose.yml` brukar `Dockerfile.pi`. `docker compose up -d --build` frå ei heilt fersk utsjekking utan `publish/` er difor ikkje nok til å bygge web-appen.

### Manuell bygging på Pi

~~~bash
git fetch origin main
git pull --ff-only origin main

dotnet restore Adrians.sln
dotnet publish Adrians/Adrians.csproj -c Release -o publish /p:UseAppHost=false

docker compose config
docker compose up -d mariadb
docker compose up -d --build web

docker compose ps
docker exec adriansvevside curl -fsS http://127.0.0.1:8080/healthz
~~~

Dette føreset at `.env` er konfigurert, at MariaDB er klar, og at Pi-verten kan køyre `dotnet publish`. På ein eksisterande installasjon kan du oppdatere berre web-containeren med `docker compose up -d --no-deps --build web` etter publish. Det reduserer risikoen for utilsikta endringar i andre tenester.

Eit alternativ er å bruke `docker build -f Dockerfile -t adriansvevside-web .`. Dette byggjer heile applikasjonen i Docker, men pass på at Compose-oppsettet er tilpassa når du vel ein annan byggjeflyt.

### Reverse proxy, TLS og nettverk

Normal trafikkflyt:

~~~text
Klient -> DNS -> ruter/brannmur -> nginx (80/443)
                                      |
                                      +-> web:8080 (ASP.NET Core)
                                             |
                                             +-> mariadb:3306
~~~

- `web` eksponerer port 8080 berre på Docker-nettverket; han blir ikkje publisert direkte frå verten av Compose.
- MariaDB er bunden til loopback-adressa på verten.
- `docker-compose.yml` definerer ein `nginx`-container med eigne namngjevne volum for konfigurasjon og sertifikat.
- **`nginx/app.conf` blir ikkje montert inn automatisk** av Compose. Konfigurasjon og sertifikat må provisionerast separat.
- Den innlagde `nginx/app.conf` nyttar `vigdal.dev` / `www.vigdal.dev`, medan URL-en over er `vigdalpi.duckdns.org`. Kontroller faktisk servernamn, sertifikat og nginx-konfigurasjon før bruk.
- Det er ingen Certbot-teneste eller automatisert sertifikatfornying definert i denne Compose-fila.
- På ein vert som alt har Nginx eller MariaDB, kontroller container-namn, portkollisjonar og eksisterande volum før du startar heile stacken.

Nginx-oppsettet må validerast med `nginx -t` i den **faktiske** nginx-containeren, ikkje berre ved å lese versjonen i repoet.

## Konfigurasjon og eksterne integrasjonar

| Nøkkel | Bruk | Lagring i utvikling / Docker |
| --- | --- | --- |
| `ConnectionStrings:DefaultConnection` | MariaDB-tilkopling | User Secrets / `ConnectionStrings__DefaultConnection` i Compose |
| `Frost:ClientId` | MET Frost | User Secrets / `FROST_CLIENT_ID` |
| `Frost:ClientSecret` | MET Frost | User Secrets / `FROST_CLIENT_SECRET` |
| `FootballData:ApiKey` | football-data.org | User Secrets / `FOOTBALLDATA_API_KEY` |
| `FootballData:ApiUrl` | URL til Barcelona-kampar | `appsettings.json` |
| `UptimeKuma:Enabled` | Slår på vising av offentleg Uptime Kuma-status | `appsettings.json` / Compose |
| `UptimeKuma:BaseUrl` | Adresse til Uptime Kuma-instansen | `appsettings.json` / Compose |
| `UptimeKuma:StatusPageSlug` | Offentleg status-side i Uptime Kuma | `appsettings.json` / Compose |
| `UptimeKuma:CacheSeconds` | Cachetid, validert til 5–300 sekund | `appsettings.json` / Compose |

Compose set Uptime Kuma på med `host.docker.internal:3001` og ein bestemt status-side-slug. Denne integrasjonen krev at tenesta er tilgjengeleg frå web-containeren. Ved lokal utvikling kan Uptime Kuma vere deaktivert.

Andre eksterne integrasjonar (met.no Nowcast, NIFS, FPL, Hacker News, SIMAS/Norkart og RSS) blir henta utan brukarstyrt API-nøkkel i koden. Tilgjenge, grenser og datakvalitet er avhengige av leverandørane.

**Deploy- og Pi-status:** `PublicPiStatusService` les `Adrians/wwwroot/status/deploy.json`. Compose monterer `./Adrians/wwwroot/status` skrivebeskytta inn i containeren. Fila må lagast av ein prosess **utanfor** applikasjonen; repoet inneheld ikkje ein ferdig deploy-agent som skriv denne fila. I mangel av gyldig fil viser dashboardet ukjend status.

## Drift og feilsøking

~~~bash
# Containerstatus og loggar
docker compose ps
docker logs --tail 100 adriansvevside
docker logs --tail 100 mariadb
docker logs --tail 100 nginx

# Liveness i web-containeren
docker exec adriansvevside curl -fsS http://127.0.0.1:8080/healthz

# Ekstern sjekk når DNS/TLS/proxy er konfigurert
curl -fsSI https://vigdalpi.duckdns.org/healthz
~~~

`/healthz` returnerer `ok` når ASP.NET Core svarar. **Dette er berre liveness**, ikkje ein test av databasen, RSS, eksterne API eller Uptime Kuma.

Ved feil:

1. Sjekk `docker compose ps` og loggane til den aktuelle tenesta.
2. Kontroller at `.env` og connection string samsvarer med MariaDB-brukaren.
3. Ver sikker på at `publish/` er oppdatert **før** Pi-imaget blir bygd.
4. Ved blanke kort: undersøk om den aktuelle eksterne API-tenesta svarar og om tilhøyrande legitimasjon er sett.
5. Ved feil på `/Digi`: sjekk status per feed og åtvaringar for `DigiController` i loggane.
6. Ved feil på HTTPS: sjekk Nginx-konfigurasjonen, DNS, sertifikat og proxy-nettverk.

**Ikkje bruk `docker compose down -v` som eit vanleg feilsøkingstiltak.** Det kan slette database- og konfigurasjonsvolum. Ta backup og ver sikker på kva volum som blir sletta.

## CI og sikkerheit

Arbeidsflytar i `.github/workflows/`:

- **CodeQL:** C#-analyse på push og PR mot `main`, nattleg plan og manuell start. GitHub Actions køyrer også restore og build i denne flyten.
- **Dependabot:** vekentleg kontroll av NuGet, Docker og GitHub Actions.
- **ZAP Baseline:** planlagd dagleg kontroll av den offentlege nettsida og manuell start. `fail_action: false` betyr at funn ikkje automatisk stoppar arbeidsflyten.
- **ChainDrop-kontroll:** manuell supply-chain-skann.

Sikkerheitsprinsipp for drift:

- Hald hemmelegheiter utanfor Git; bruk User Secrets lokalt og miljøvariablar eller tryggare løysing for produksjon.
- Ikkje opne MariaDB, Uptime Kuma-administrasjon, Docker-socket eller andre administrasjonsgrensesnitt mot internett.
- Avgrens tilgangen til reverse proxy, oppdater base-image og bibliotek jamleg, og overvåk feil i CI og containerloggar.
- Gå gjennom tredjeparts-RSS og eksterne API med tanke på feilsituasjonar, timeout og endringar i responsformat.
- Ver merksam på at `/healthz` ikkje dokumenterer full applikasjonshelse.

### Kjende avgrensingar og teknisk gjeld

Desse punkta er observerte i repoet og er **ikkje** automatisk utbetra av dokumentasjonsendringa:

- Autentisering/autorisasjon for `RPG` og `Game` er ikkje ferdig konfigurert i `Program.cs`.
- Det er ikkje ein eigen automatisert testprosjektstruktur i løysinga.
- `nginx/app.conf` er ikkje bunden til Compose og kan avvike frå faktisk produksjonskonfigurasjon.
- Deploy-script, sertifikatfornying og generering av `deploy.json` ligg utanfor repoet.
- `Adrians/nuget.config` har ei ekstra pakkekjelde som kan krevje legitimasjon ved restore.
- `wwwroot/.well-known/Security.txt` peikar i `Canonical` til ei eldre Azure-adresse. Kontroller at offentleg `security.txt` peikar til gjeldande domene.
- Fleire eldre controllerar brukar meir direkte API-kall og har ikkje like gjennomført feiltoleranse som dashboardet og den nye RSS-aggregatoren.
- Kontroller at sikkerheitsheaderar, forwarded headers og HTTPS-omdirigering fungerer med den faktiske proxy-konfigurasjonen.

## Bidrag og vedlikehald

Lag eiga grein og pull request. Før fletting bør du minst kontrollere:

~~~bash
dotnet restore Adrians.sln
dotnet build Adrians.sln -c Release
~~~

Test relevante ruter manuelt når endringa krev nettverk eller eksterne API. For endringar i database, Docker eller Nginx bør du òg verifisere backup, oppstart og rollback.

## Lisens

MIT. Sjå [LICENSE](LICENSE).
