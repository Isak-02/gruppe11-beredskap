# KartKrise – Ressurs- og behovsportal

## Om prosjektet

Dette prosjektet utvikles som en del av IS-20X Case 2026.

KartKrise er en webløsning for kriseberedskap i Totalforsvaret. Offentlige aktører kan registrere behov for hjelp, og privatpersoner, organisasjoner og bedrifter kan registrere ressurser de har tilgjengelig. Behov og ressurser vises i kart, og en koordinator kan koble dem sammen og følge dem til de er løst.

## Funksjonalitet

**Brukere og innlogging**
- Registrering av ny bruker (navn, e-post, telefon og passord). Brukeren lagres i tabellen `AspNetUsers` i MariaDB.
- Innlogging og utlogging med ASP.NET Core Identity
- Glemt passord og tilbakestilling av passord med engangskode
- Tre roller med ulik tilgang (se [Roller og tilgang](#roller-og-tilgang))
- Min profil og Innstillinger for innlogget bruker
- Menyen viser bare det rollen har tilgang til

**Behov og ressurser**
- Registrering av behov og ressurser i et skjema med flere steg (veiviser), med oppsummering før lagring
- Endring og sletting av egne ressurser
- *Mine behov* og *Mine ressurser* viser det brukeren selv har registrert
- *Alle behov* og *Alle ressurser* for koordinator
- Validering av påkrevde felt med norske feilmeldinger, både i nettleseren og på serveren

**Kart**
- Kart med Leaflet og OpenStreetMap
- Posisjon settes med markør i kartet (Leaflet.Draw), og for behov også med sirkel (radius)
- Norske tekster i verktøylinjen
- *Ressurser per fylke* (bare koordinator): velg fylke i lista eller ved å klikke i kartet. Bare tilgjengelige ressurser vises, og lista kan skrives ut

**Kobling (`/Kobling`)**
- Kart og lister over åpne behov og ledige ressurser
- Behov vises med fargekode etter prioritet: rød (høy), oransje (middels) og gul (lav)
- Fullskjerm og tegnforklaring på kartet
- Statusflyt for behov: **Ny → Vurderes → Tildelt → Løst**. Ressursen blir *Tildelt* når den kobles, og *Tilgjengelig* igjen når behovet er løst.

## Roller og tilgang

| Rolle | Hvem | Kan |
|---|---|---|
| KriseInnmelder | Offentlig aktør (kommune, politi, brann, helse, Forsvaret o.l.) | Registrere behov og se egne behov |
| RessursInnmelder | Privatperson, bedrift, frivillig organisasjon | Registrere, endre og slette egne ressurser |
| Koordinator | Kriseledelsen | Alt over, i tillegg til *Alle behov*, *Alle ressurser*, *Ressurser per fylke* og `/Kobling` |

Ved registrering får man alltid rollen RessursInnmelder. KriseInnmelder og Koordinator opprettes av oss, slik at fremmede ikke kan gi seg selv tilgang til behov og koblinger.

## Testbrukere

Når appen kjører i Development (både med Rider og med Docker Compose), opprettes det automatisk én testbruker per rolle:

| Rolle | E-post | Passord |
|---|---|---|
| Koordinator | `koordinator@kartkrise.local` | `Kartkrise123!` |
| KriseInnmelder | `krise@kartkrise.local` | `Kartkrise123!` |
| RessursInnmelder | `ressurs@kartkrise.local` | `Kartkrise123!` |

Passordet ligger i `appsettings.Development.json` under `Testbrukere:Passord`. Testbrukerne opprettes ikke i produksjon.

### Slik logger du inn

1. Trykk **Logg inn** øverst til høyre, eller bla ned til innloggingsboksen på forsiden.
2. Skriv inn e-post og passord, og trykk **Logg inn**.
3. Profilmenyen øverst til høyre viser navn og rolle, og har lenker til *Mine behov* / *Mine ressurser* og **Logg ut**.
4. Ny bruker: trykk **Opprett bruker** i innloggingsboksen og fyll ut skjemaet.
5. Glemt passord: trykk **Glemt passord?**. Lenken for nytt passord skrives i loggen (se [Drift](#drift)).

## Teknologistack

- ASP.NET Core MVC (.NET 10) og C#
- Razor Views
- ASP.NET Core Identity
- Entity Framework Core 9 (Pomelo MySQL-provider)
- MariaDB 11
- Docker og Docker Compose
- Leaflet, Leaflet.Draw og OpenStreetMap
- Bootstrap
- xUnit
- JetBrains Rider og GitHub

## Prosjektstruktur

```text
gruppe11-beredskap/
├── Properties/                    launchSettings.json (kjøreprofiler for Rider)
├── wwwroot/                       css, js, bilder og data/fylker.json
├── Controllers/                   AccountController, BehovController, RessursController, MatchingController, HomeController
├── DataAccess/                    ApplicationDbContext
│   └── Repositories/              BehovRepository, RessursRepository, MatchingRepository (med interface)
├── Migrations/                    EF Core-migrasjoner
├── Models/                        View-modeller (Behov, Ressurs, Matching, Account) og statusnavn
│   ├── Authorization/             RoleNames: KriseInnmelder, RessursInnmelder og Koordinator
│   └── Entities/                  Behov, Ressurs, Matching, Bruker (tabellene i databasen)
├── Services/                      IdentitySeeder, ConsoleEmailSender, NorskIdentityErrorDescriber
├── Views/                         Account, Behov, Ressurs, Matching, Home, Shared
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
└── Program.cs

gruppe11-beredskap.Tests/          xUnit-tester

compose.yaml                       Docker Compose (app + MariaDB), ligger i rotmappen
.env.example                       Eksempel på passord og navn til databasen
```

## Installasjon og kjøring

### Krav

- Docker Desktop
- .NET 10 SDK (bare for å kjøre i Rider eller kjøre testene)

### Kloning av repositoriet vårt

```bash
git clone https://github.com/Isak-02/gruppe11-beredskap
cd gruppe11-beredskap
```

### Alternativ 1: Docker Compose (app og database i Docker)

```bash
docker compose up -d --build
```

Åpne http://localhost:8080 i nettleseren og logg inn med en av [testbrukerne](#testbrukere).

### Alternativ 2: Run-knappen i JetBrains Rider

1. Start bare databasen: `docker compose up -d db`
2. Åpne `gruppe11-beredskap.sln` i Rider
3. Trykk på **Run** (▶). Appen åpnes på http://localhost:5178 (eller https://localhost:7287)

Tabellene, rollene og testbrukerne opprettes automatisk når appen starter.

## Drift

| Hva | Kommando |
|---|---|
| Starte app og database | `docker compose up -d --build` |
| Se om containerne kjører | `docker compose ps` |
| Se loggen til appen | `docker compose logs -f gruppe11-beredskap` |
| Stoppe (data beholdes) | `docker compose down` |
| Stoppe og slette all data | `docker compose down -v` |

- **Porter:** appen bruker 8080 og databasen 3306. Er 3306 opptatt (for eksempel av en lokal MySQL), må den stoppes først.
- **Data** lagres i Docker-volumet `db_data`, så den blir liggende når containerne stoppes.
- **Migrasjoner** kjøres automatisk ved oppstart (`Database.Migrate()` i `Program.cs`). Ny migrasjon lages med `dotnet ef migrations add NavnPaaMigrasjon --project gruppe11-beredskap`.
- **Passord og databasenavn** har standardverdier i `compose.yaml`. De kan overstyres ved å kopiere `.env.example` til `.env` og endre verdiene.
- **Glemt passord:** appen sender ikke ekte e-post. Lenken for å tilbakestille passordet skrives i loggen (`docker compose logs gruppe11-beredskap` eller Run-vinduet i Rider).

### Innlogging til databasen

Standardverdiene fra `compose.yaml`, for å koble til databasen med for eksempel Rider (Database-vinduet), DBeaver eller HeidiSQL:

| Felt | Verdi |
|---|---|
| Server | `localhost` |
| Port | `3306` |
| Database | `beredskapdb` |
| Bruker | `beredskap` |
| Passord | `gruppe11` |
| Root-passord | `gruppe11` |

Inne i Docker kobler appen seg til med servernavnet `db` i stedet for `localhost`. Er verdiene overstyrt i en `.env`-fil, gjelder de i stedet.

## Systemarkitektur

```text
Nettleser (Razor-sider, Leaflet-kart)
    │  GET / POST (med antiforgery-token)
    ▼
Controllers  ──  [Authorize(Roles = ...)] sjekker innlogging og rolle
    │  view-modeller med validering
    ▼
Repositories (DataAccess/Repositories)
    │
    ▼
ApplicationDbContext (EF Core + Identity)
    │
    ▼
MariaDB i Docker
```

- **MVC:** Nettleseren sender GET og POST til en controller. Controlleren validerer view-modellen, bruker et repository til å lese eller lagre, og sender data til et Razor-view.
- **Kart:** Når man tegner på kartet, lagres posisjon (og eventuell radius) i skjulte felt som sendes med skjemaet. På koblingssiden sendes behov og ressurser til kartet som JSON. På *Ressurser per fylke* brukes posisjonen til å finne hvilket fylke ressursen ligger i (`wwwroot/data/fylker.json`).
- **Repositories:** Controllerne snakker med interface (`IBehovRepository`, `IRessursRepository`, `IMatchingRepository`), ikke direkte med databasen. Det gjør at testene kan bruke falske repositories.
- **Identity:** `ApplicationDbContext` arver fra `IdentityDbContext<Bruker>`, slik at brukere og roller lagres i den samme databasen som behov og ressurser. `Bruker` arver fra `IdentityUser` og har i tillegg `Navn`.
- **Eierskap:** Behov og ressurser lagrer hvem som opprettet dem (`OpprettetAvId`). Det brukes til *Mine*-sidene. Ressurser kan bare endres og slettes av eieren eller en koordinator.
- **Database:** Tabellene `Behov`, `Ressurser` og `Matchinger`, i tillegg til Identity-tabellene (`AspNetUsers`, `AspNetRoles`, `AspNetUserRoles` m.fl.).

## Sikkerhet

| Tiltak | Hvordan |
|---|---|
| Hashede passord | Identity lagrer bare en hash (PBKDF2 med HMAC-SHA512, 100 000 runder og unikt salt) i `AspNetUsers.PasswordHash` |
| Passordkrav | Minst 6 tegn med stor og liten bokstav, tall og spesialtegn |
| Låsing av konto | Kontoen låses etter 10 feil innloggingsforsøk |
| Roller og tilgang | `[Authorize(Roles = ...)]` på controllerne. Uten riktig rolle får man *Ingen tilgang*. |
| Eiersjekk | Ressurser kan bare endres og slettes av eieren eller en koordinator |
| Ingen selvvalgt rolle | Registrering gir alltid RessursInnmelder |
| CSRF-beskyttelse | `[ValidateAntiForgeryToken]` på alle POST-handlinger |
| XSS-beskyttelse | Razor gjør om tekst før den vises. I kartet brukes `textContent` i stedet for HTML. |
| SQL-injeksjon | EF Core bruker parametre i alle spørringer |
| Trygge videresendinger | Etter innlogging sendes man bare videre til adresser på egen side (`Url.IsLocalUrl`) |
| HTTPS | `UseHttpsRedirection`, og HSTS utenfor Development |

## Testing

Vi har gjennomført enhetstester, systemtester, sikkerhetstester og brukervennlighetstester. Resultatene står i tabellene under.

### Enhetstester

Enhetstester med xUnit ligger i `gruppe11-beredskap.Tests`. Testene bruker falske repositories, så de trenger ingen database. Kjør dem i Rider eller med `dotnet test`.

| Testklasse | Hva den sjekker | Antall |
|---|---|---|
| `BehovControllerTests` | Registrering, *Mine behov* og oversikt av behov, inkludert radius, beskrivelse og hvem som opprettet | 13 |
| `RessursControllerTests` | Registrering, endring, sletting, oversikt og ressurser per fylke, krav om drivstoff og lastekapasitet | 11 |
| `BehovViewModelTests` | Validering av radius, posisjon og beskrivelse | 8 |
| `MatchingControllerTests` | Visning av åpne behov og ledige ressurser, og lagring av koblinger | 5 |
| `TilgangTests` | Rollekrav på registrering, oversikt, ressurser per fylke og kobling | 8 |

Siste kjøring (6. november 2026): 45 bestått, 0 feilet.

### Systemtest

Gjennomført manuelt med appen kjørende i Docker Compose.

| # | Scenario | Forventet resultat | Resultat |
|---|---|---|---|
| S1 | Registrer ny bruker på forsiden | Brukeren logges inn og finnes i `AspNetUsers` med rollen RessursInnmelder | Godkjent |
| S2 | Logg inn og ut med hver testbruker | Menyen viser riktige valg for rollen | Godkjent |
| S3 | Registrer ressurs med markør i kartet | Ressursen vises under *Mine ressurser* | Godkjent |
| S4 | Endre og slett ressursen | Endringen lagres, og ressursen forsvinner etter sletting | Godkjent |
| S5 | Registrer behov som KriseInnmelder | Behovet vises under *Mine behov* og på `/Kobling` | Godkjent |
| S6 | Koble behov og ressurs, og marker som løst | Status går Ny → Vurderes → Tildelt → Løst, og ressursen blir ledig igjen | Godkjent |
| S7 | Glemt passord | Lenken skrives i loggen, og nytt passord virker | Godkjent |
| S8 | Velg fylke på *Ressurser per fylke* | Bare tilgjengelige ressurser i fylket vises, og lista kan skrives ut | Godkjent |

S8 (6. november 2026, Docker på localhost:8080, innlogget som koordinator): Agder viste de tre tilgjengelige ressursene. Oslo viste ingen. Klikk på fylket i kartet valgte Agder. Skriv ut viste overskrift og tabell, uten meny, kart og filter.

### Sikkerhetstest

| # | Scenario | Forventet resultat | Resultat |
|---|---|---|---|
| T1 | Åpne `/Ressurs/Registrer` uten å være logget inn | Sendes til innlogging | Godkjent |
| T2 | Åpne `/Kobling` som RessursInnmelder eller KriseInnmelder | *Ingen tilgang* | Godkjent |
| T3 | Åpne `/Ressurs/Endre/{id}` for en ressurs en annen bruker eier | *Ingen tilgang* | Godkjent |
| T4 | Se på `PasswordHash` i `AspNetUsers` | En lang hash, ikke passordet | Godkjent |
| T5 | Logg inn med feil passord 10 ganger | Kontoen låses en periode | Godkjent |
| T6 | Skriv `<script>alert(1)</script>` i beskrivelsen | Teksten vises som tekst, skriptet kjøres ikke | Godkjent |

### Brukervennlighetstest

Gjennomført med personer utenfor gruppa, og med mobilvisning i nettleseren (F12 → mobilikonet).

| # | Oppgave | Hva vi så etter | Resultat |
|---|---|---|---|
| B1 | Registrer en ressurs på smal skjerm | Om skjemaet og kartet er lett å bruke på mobil | Godkjent |
| B2 | Sett markør i kartet | Om det er tydelig hvordan man velger posisjon | Godkjent |
| B3 | Finn igjen ressursen du registrerte | Om *Mine ressurser* er lett å finne | Godkjent |
| B4 | Les oversikten og koblingssiden | Om fargene og tegnforklaringen er forståelige | Godkjent |

B1 (6. november 2026, 390 px bredde): veiviseren og kartverktøyet fikk plass uten vannrett scrolling. En ressurs (Lokaler) ble lagret med markør og vises under *Mine ressurser*.

## Bruk av KI

KI ble brukt som hjelp fra idé til koding. All kode er lest og tilpasset av gruppa.

### Bruksområder

- Forklare MVC-flyten mellom controller, view-modell og view
- Foreslå skjema, validering og hvordan kartklikk kan fylle skjulte felt
- Hjelp med Leaflet.Draw, Identity og roller, database og migrasjoner
- Skrive og forbedre enhetstester
- Hjelp med Git (grener, konflikter, commits og pull requests)
- Hjelp med dokumentasjon
- Finne mangler i prosjektet og få tilbakemeldinger

### Verktøy

- JetBrains Rider med AI Assistant, Claude, ChatGPT, Copilot.

### Eksempler på prompt-kommandoer

- «Hvordan kan vi løse dette?: Når man velger posisjon i kart, og velger en ny posisjon skal forrige posisjon bli slettet»
- «Hvorfor får jeg ikke til å pushe til branch? Her er feilmeldingene: X»
- «Gi en tilbakemelding på filstruktur, og kom med forslag til hvordan den kan forbedres.»
- «Vurder prosjektet vårt opp mot oppgavebeskrivelsen på innleveringen.»

## Kjente begrensninger

- KriseInnmelder ser bare egne behov, ikke ledige ressurser.
- Behov kan registreres, men ikke endres eller slettes.
- Sletting av konto er ikke implementert.
- Appen sender ikke ekte e-post. Lenker skrives i loggen.

## Team

Gruppe 11

- Noah Falkum
- Mads André Skjervik Knutsen
- Emal Mir
- Vetle Mikael With Nygaard
- Isak Øgreid Næss
- Filip Myhren Ormestad

## Status

- **Innlevering 1 (IS-202):** MVC-app i Docker med skjema, GET/POST og kart. Data ble lagret i minnet.
- **Innlevering 2:** Lagring i MariaDB, app og database i Docker Compose, innlogging og registrering med Identity, tre roller, endring og sletting av ressurser, kart med verktøylinje og radius, ressurser per fylke, koblingsside med statusflyt, nytt design og 45 enhetstester.

GitHub-lenke: https://github.com/Isak-02/gruppe11-beredskap

## Lisens

Se LICENSE-filen for mer informasjon.