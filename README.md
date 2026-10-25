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

Feed-lista er hardkoda i `Adrians/Controllers/DigiController.cs`. For å endre kjelder, endrar du lista der. Den gamle `RssFeedService` er fjerna. Digi har si eiga RSS-innhenting.

## Teknologi

- **.NET 10 / ASP.NET Core MVC / Razor**
- **Bootstrap 5** og eigen CSS/JavaScript
- **IHttpClientFactory**, `IMemoryCache` og eksterne REST-/RSS-tenester
- **Docker / Docker Compose**, `Dockerfile` og `Dockerfile.pi`
- **Nginx**, HTTPS-sertifikat frå Let's Encrypt og DNS handtert utanfor applikasjonen
- **GitHub Actions**, Dependabot og CodeQL

## Prosjektstruktur

~~~text
Adrians.sln
Adrians/
  Adrians.csproj
  Program.cs                  Oppstart, DI, HTTP-klientar, cache og ruting
  Controllers/                MVC-endepunkt og Digi sin RSS-aggregator
  Services/                   Vêr, fotball, tømmekalender, Uptime Kuma, Pi-status m.m.
  Resources/                  API-klientar og konfigurasjonsmodellar
  Models/                     Domene- og API-modellar
  ViewModels/                 Visingsmodellar
  Views/                      Razor-visingar og felles layout
  wwwroot/                    CSS, JS, bilete, vêr-ikon og offentlege statusfiler
  appsettings.json            Ikkje-sensitive standardverdiar
Dockerfile                    Bygg og publish med .NET 10 SDK i Docker
Dockerfile.pi                 Berre runtime; krev lokal publish/-mappe
docker-compose.yml            web og nginx
nginx/app.conf                Døme på proxy-konfigurasjon (ikkje automatisk montert)
.env.example                  Utgangspunkt for lokal Docker-konfigurasjon
.github/workflows/            CodeQL, ZAP og manuell tryggleiksskann
~~~

## Føresetnader

For lokal utvikling:

- .NET **10 SDK**
- Git
- Docker med Docker Compose ved behov for containerkøyring
- Eventuelt Visual Studio eller VS Code

For Raspberry Pi-drift treng du òg Docker Engine/Compose, .NET 10 SDK på verten dersom du brukar `Dockerfile.pi`-løpet, ein fungerande reverse proxy og DNS/TLS-oppsett. Docker-imaga for .NET 10 må støtte arkitekturen på verten.

## Lokal utvikling (Windows / PowerShell)

Du treng .NET 10 SDK og Git. Ingen database er nødvendig.

```powershell
git clone https://github.com/Vigdals/AdriansVevside.git
cd AdriansVevside
cd Adrians
dotnet user-secrets set "Frost:ClientId" "<klient-id>"
dotnet user-secrets set "Frost:ClientSecret" "<klienthemmelegheit>"
dotnet user-secrets set "FootballData:ApiKey" "<api-nokkel>"
cd ..
dotnet restore .\Adrians.sln
dotnet build .\Adrians.sln -c Debug
dotnet run --project .\Adrians\Adrians.csproj
```

Dei tre API-nøklane trengst berre for Frost og Barca. Andre eksterne integrasjonar kan svare utan nøklar. Utviklingsprofilen brukar `https://localhost:7289` og `http://localhost:5063`.

## Docker og Raspberry Pi

`docker-compose.yml` inneheld no berre `web` og `nginx`. MariaDB er ikkje lenger ein del av prosjektet. Eksisterande Docker-datavolum blir ikkje sletta av kodeendringa.

`Dockerfile` byggjer heile prosjektet i Docker. `Dockerfile.pi` brukar ein ferdig lokal `publish/`-katalog og har helsesjekk mot `/healthz`. Compose nyttar `Dockerfile.pi`.

```bash
cp .env.example .env
# Set API-nøklar ved behov.
dotnet restore Adrians.sln
dotnet publish Adrians/Adrians.csproj -c Release -o publish /p:UseAppHost=false
docker compose config
docker compose up -d --build web
docker exec adriansvevside curl -fsS http://127.0.0.1:8080/healthz
```

På eksisterande vert er det tryggare å oppdatere berre web-appen enn å starte heile stacken på nytt. Nginx-konfigurasjon, TLS-sertifikat og eventuelt eksisterande nettverk må kontrollerast på verten. `nginx/app.conf` er ikkje automatisk montert av Compose.

### Innstillingar

| Nøkkel | Bruk |
| --- | --- |
| `Frost:ClientId` / `Frost:ClientSecret` | Snødata frå MET Frost |
| `FootballData:ApiKey` | Barcelona-kampar |
| `FootballData:ApiUrl` | URL for football-data.org |
| `UptimeKuma:Enabled` | Slå på tenestestatus |
| `UptimeKuma:BaseUrl` | Adresse til Uptime Kuma |
| `UptimeKuma:StatusPageSlug` | Offentleg statusside |
| `UptimeKuma:CacheSeconds` | Cachetid |

Produksjonsvariablar for API-nøklar blir sett i `.env` og vidareført av Compose. Ikkje legg verkelege hemmelegheiter i repoet.

Dei aktive sidene hentar elles data frå met.no, NIFS, FPL, Hacker News, SIMAS/Norkart og RSS-kjelder. `PublicPiStatusService` les `Adrians/wwwroot/status/deploy.json` som blir produsert utanfor applikasjonen. Fila er valfri og manglande status blir vist som ukjend.

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
- Ikkje opne Uptime Kuma-administrasjon, Docker-socket eller andre administrasjonsgrensesnitt mot internett.
- Avgrens tilgangen til reverse proxy, oppdater base-image og bibliotek jamleg, og overvåk feil i CI og containerloggar.
- Gå gjennom tredjeparts-RSS og eksterne API med tanke på feilsituasjonar, timeout og endringar i responsformat.
- Ver merksam på at `/healthz` ikkje dokumenterer full applikasjonshelse.

### Kjende avgrensingar og teknisk gjeld

Desse punkta er observerte i repoet og er **ikkje** automatisk utbetra av dokumentasjonsendringa:

- Autentisering/autorisasjon for `RPG` og `Game` er ikkje ferdig konfigurert i `Program.cs`.
- Det er ikkje ein eigen automatisert testprosjektstruktur i løysinga.
- `nginx/app.conf` er ikkje bunden til Compose og kan avvike frå faktisk produksjonskonfigurasjon.
- Deploy-script, sertifikatfornying og generering av `deploy.json` ligg utanfor repoet.
- `wwwroot/.well-known/Security.txt` peikar i `Canonical` til ei eldre Azure-adresse. Kontroller at offentleg `security.txt` peikar til gjeldande domene.
- Nokre aktive controllerar brukar direkte API-kall og har ikkje like gjennomført feiltoleranse som dashboardet og RSS-aggregatoren.
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
