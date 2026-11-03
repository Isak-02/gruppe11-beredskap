# KartKrise – Ressurs- og behovsportal for kriseberedskap

## Om prosjektet

Dette prosjektet utvikles som en del av IS-20X Case 2026.

KartKrise er en webbasert løsning som skal støtte kriseberedskap og samhandling i Totalforsvaret. Løsningen gjør det mulig for offentlige aktører å registrere behov for bistand, samtidig som privatpersoner, organisasjoner og bedrifter kan registrere tilgjengelige ressurser. Både behov og ressurser knyttes til en geografisk posisjon i et kart, slik at en kriseleder kan få oversikt over situasjonen og koble behov med egnede ressurser.

## Funksjonalitet

### Ferdig implementert

- **Forside:** Egen forside for KartKrise med logo, bakgrunnsbilde og snarvei til registrering av ressurser.
- **Registrering av behov:** Skjema (GET/POST) med type behov, beskrivelse (vises etter at type er valgt), tidspunkt/frist, prioritet (Lav, Middels, Høy), kontaktpunkt og posisjon i kart. Registrerte behov vises på en egen oversiktsside (`/Behov/Oversikt`).
- **Registrering av ressurser:** Skjema (GET/POST) med type ressurs (f.eks. drone, traktor, lastebil, aggregat, båt, mannskap), beskrivelse, eventuell egendefinert type («Annet»), drivstofftype, lastekapasitet, tidspunkt, kontaktpunkt og posisjon i kart. Registrerte ressurser vises på en egen oversiktsside (`/Ressurs/Oversikt`).
- **Kart:** Leaflet-kart med OpenStreetMap og verktøylinje (Leaflet.Draw) på registreringssidene. For ressurser settes posisjonen med en markør, og for behov med en markør eller en sirkel. Breddegrad, lengdegrad og eventuell radius (meter) lagres sammen med skjemaet.
- **Kobling (matching):** Egen side på `/Kobling` (den gamle adressen `/Matching` virker fortsatt) der behov og ressurser som ikke allerede er koblet kan kobles sammen. Siden viser:
  - et kart med åpne behov (utropstegn-ikon med farge etter prioritet: rød = høy, oransje = middels, gul = lav) og ledige ressurser, med popup-informasjon
  - tabeller over åpne behov og ledige ressurser, inkludert beskrivelser
  - en liste over eksisterende koblinger med tidspunkt
- **Validering:** Påkrevde felt valideres før data lagres. I tillegg kreves posisjon i kartet, radius må være større enn 0, beskrivelser kan være maks 1000 tegn, kjøretøy og maskiner må ha drivstofftype, og ressurser som lastebil, henger og traktor må ha lastekapasitet større enn 0.
- **Lagring i database:** Data lagres i MariaDB via Entity Framework Core og repository-klasser.
- **Responsivt grensesnitt:** Bootstrap med eget KartKrise-design (navigasjonsmeny, farger og knapper).

### Under utvikling

- **Innlogging og brukerkontoer:** Det er laget en *designprototyp* for innlogging, opprettelse av bruker og profilmeny (med knappen «Test profil» i navigasjonsmenyen). Dette er foreløpig kun brukergrensesnitt: skjemaene er deaktivert, ingen brukere lagres, og profilmenyen viser en eksempelbruker. Selve autentiseringen (planlagt med ASP.NET Core Identity) er ikke implementert.
- **Tilgangskontroll:** Koblingssiden skal kun være tilgjengelig for kriseleder. Dette legges på når innlogging er på plass, og siden er foreløpig åpen for alle.

## Teknologistack

- ASP.NET Core MVC (.NET 10)
- C#
- Razor Views
- Entity Framework Core 9 med Pomelo MySQL-provider (`Pomelo.EntityFrameworkCore.MySql`)
- MariaDB 11
- Docker og Docker Compose
- Leaflet 1.9.4, Leaflet.Draw 1.0.4 og OpenStreetMap
- Bootstrap og jQuery (inkl. jQuery Validation for klientvalidering)
- xUnit (med Microsoft.NET.Test.Sdk og coverlet)
- JetBrains Rider
- Git og GitHub

## Prosjektstruktur

```text
gruppe11-beredskap/                (rotmappe)
├── gruppe11-beredskap.sln
├── compose.yaml                   Docker Compose: webapp + MariaDB
├── .env.example                   Eksempel på miljøvariabler for databasen
├── gruppe11-beredskap/            Webapplikasjonen
│   ├── Controllers/
│   │   ├── HomeController.cs      Forside og feilside
│   │   ├── BehovController.cs     Registrering og oversikt over behov
│   │   ├── RessursController.cs   Registrering og oversikt over ressurser
│   │   └── MatchingController.cs  Kobling av behov og ressurser (/Kobling)
│   ├── DataAccess/
│   │   ├── ApplicationDbContext.cs
│   │   └── Repositories/          IBehovRepository, IRessursRepository, IMatchingRepository
│   │                              og tilhørende implementasjoner
│   ├── Migrations/                EF Core-migrasjoner (InitialCreate, LeggTilBehov, LeggTilMatching,
│   │                              LeggTilRadius, LeggTilBeskrivelse, LeggTilBeskrivelsePaaRessurs)
│   ├── Models/
│   │   ├── BehovViewModel.cs
│   │   ├── RessursViewModels.cs   (RessursViewModel)
│   │   ├── MatchingViewModel.cs   (MatchingViewModel og MatchingVisning)
│   │   ├── ErrorViewModel.cs
│   │   └── Entities/              Behov, Ressurs, Matching (tabellene i databasen)
│   ├── Views/
│   │   ├── Home/                  Forside med innloggingsprototyp
│   │   ├── Behov/                 Registrer, Oversikt
│   │   ├── Ressurs/               Registrer, Oversikt
│   │   ├── Matching/              Index (koblingssiden)
│   │   └── Shared/                _Layout (navigasjon og profilmeny), Error, valideringsskript
│   ├── wwwroot/                   css, js, images (logo og bakgrunn), lib (Bootstrap, jQuery)
│   ├── Properties/launchSettings.json
│   ├── appsettings.json / appsettings.Development.json
│   ├── Program.cs                 Oppsett av tjenester, database, migrasjoner og ruter
│   └── Dockerfile
└── gruppe11-beredskap.Tests/      xUnit-tester
```

## Installasjon

### Forutsetninger

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (med Docker Compose)
- [.NET 10 SDK](https://dotnet.microsoft.com/) og JetBrains Rider (kun nødvendig for å kjøre appen utenfor Docker)

### Klon repositoriet

```bash
git clone https://github.com/Isak-02/gruppe11-beredskap
cd gruppe11-beredskap
```

### Alternativ 1: Start med Docker Compose

Starter både webappen og MariaDB-databasen i containere:

```bash
docker compose up -d --build
```

Åpne deretter:

```text
http://localhost:8080
```

Webappen venter til databasen er klar (healthcheck), og migrasjonene kjøres automatisk ved oppstart, slik at tabellene opprettes første gang. Data lagres i Docker-volumet `db_data`.

Databaseinnstillingene har standardverdier i `compose.yaml`. For å bruke egne verdier kan `.env.example` kopieres til `.env` og tilpasses:

```bash
cp .env.example .env
```

Stoppe containerne:

```bash
docker compose down
```

### Alternativ 2: Kjør direkte fra JetBrains Rider (Run-knappen)

Når appen kjøres fra Rider, kobler den seg til MariaDB på `localhost:3306` via connection string i `appsettings.Development.json`. Databasen må derfor kjøre først:

1. Start kun databasecontaineren:

   ```bash
   docker compose up -d db
   ```

2. Åpne `gruppe11-beredskap.sln` i JetBrains Rider.
3. Velg kjørekonfigurasjonen **http** (eller **https**) for prosjektet `gruppe11-beredskap` øverst i Rider.
4. Trykk på **Run**-knappen (▶). Nettleseren åpnes automatisk på:

   ```text
   http://localhost:5178
   ```

   (Med **https**-profilen: `https://localhost:7287`.)

Migrasjonene kjøres automatisk også her. Merk: Hvis du har endret databaseinnstillingene i `.env`, må connection string i `appsettings.Development.json` stemme overens med disse.

## Systemarkitektur

- Nettleseren sender GET og POST til en ASP.NET Core MVC-controller. Controlleren validerer view-modellen og sender den til et Razor-view.

- BehovController og RessursController har hver en GET-action som viser skjema og kart, en POST-action som lagrer, og en GET-action (`Oversikt`) som viser dataene. Når en markør eller sirkel tegnes, flyttes eller slettes med verktøylinjen i Leaflet-kartet, oppdateres breddegrad, lengdegrad og radius i skjulte felt, og de følger med i POST.

- MatchingController har en GET-action (`Index`) som henter åpne behov, ledige ressurser og eksisterende koblinger, og en POST-action (`Koble`) som lagrer en ny kobling. Siden nås via ruten `/Kobling`, som er definert i `Program.cs`.

- Controllerne bruker repository-grensesnitt (`IBehovRepository`, `IRessursRepository`, `IMatchingRepository`) som registreres med dependency injection i `Program.cs`. Controllerne vet dermed ikke hvilken database som ligger bak, noe som også gjør det enkelt å teste dem med falske repositories.

- Data lagres i MariaDB (i Docker) via Entity Framework Core. Repository-klasser i `DataAccess` håndterer lagring og henting, og migrasjonene oppretter og oppdaterer tabellene automatisk når appen starter. Tabellen for koblinger har fremmednøkler til behov og ressurser.

- Appen bruker kulturen `en-US`, slik at koordinater med punktum som desimaltegn fra kartet (JavaScript) tolkes riktig.

## Testing

Automatiske enhetstester med xUnit ligger i prosjektet `gruppe11-beredskap.Tests`. Testene bruker falske repositories i minnet (`FakeBehovRepository`, `FakeRessursRepository`, `FakeMatchingRepository`), så de kjører uten database.

Testene kan kjøres fra Rider (Unit Tests-vinduet) eller fra terminalen i rotmappen:

```bash
dotnet test
```

| Testklasse | Hva den sjekker | Antall | Resultat |
|---|---|---|---|
| `BehovControllerTests` | GET og POST `Registrer` og `Oversikt` for behov: at gyldige behov lagres og sender videre til oversikten, at ugyldige ikke lagres, at radius lagres for sirkel og er tom for markør, og at radius og beskrivelse vises i oversikten | 10 | Bestått |
| `RessursControllerTests` | GET og POST `Registrer` og `Oversikt` for ressurser, og at kjøretøy må ha drivstofftype og lastebiler lastekapasitet | 6 | Bestått |
| `BehovViewModelTests` | Valideringen i `BehovViewModel`: at radius er valgfri, at radius må være større enn 0, at posisjon på kartet er påkrevd, og at beskrivelse er påkrevd og maks 1000 tegn | 8 | Bestått |
| `MatchingControllerTests` | At koblingssiden bare viser åpne behov og ledige ressurser, at koordinater og beskrivelser sendes med til kartet, at en gyldig kobling lagres, og at en kobling uten valg ikke lagres | 5 | Bestått |

Resultat av siste kjøring (`dotnet test`, 3. november 2026): 29 bestått, 0 feilet, 0 hoppet over.

## Bruk av KI

KI ble brukt som hjelp fra idé til koding og dokumentasjon. All kode er lest, vurdert og tilpasset av gruppa.

### Bruksområder

- Forklare MVC-flyten mellom controller, view-modell og view
- Foreslå skjema, validering og hvordan kartklikk kan fylle skjulte felt
- Hjelp med oppsett av Entity Framework Core, MariaDB, migrasjoner og repository-mønsteret
- Hjelp med Leaflet.Draw og visning av behov og ressurser i kartet
- Skrive og forbedre enhetstester
- Finne mangler i oppgaven
- Få tilbakemeldinger
- Opprette klasser
- Oppdatere README-filen basert på den faktiske tilstanden i prosjektet

### Verktøy

- JetBrains Rider med AI Assistant, Claude (inkl. Claude Code), ChatGPT, Copilot

### Eksempler på prompt-kommandoer

- «Hva mangler vi mot disse kravene: controller, view model, view, GET/POST, skjema, kart, Docker og dokumentasjon?»
- «Hvordan kan vi løse dette?: Når man velger posisjon i kart, og velger en ny posisjon skal forrige posisjon bli slettet»
- «Hvorfor får jeg ikke til å pushe til branch? Her er feilmeldingene: X»
- «Gi en tilbakemelding på filstruktur, og kom med forslag til hvordan den kan forbedres.»
- «Gå gjennom hele prosjektet og oppdater README-filen slik at den gjenspeiler funksjonene, teknologiene og testene som faktisk er implementert.»

## Team

Gruppe 11

- Noah Falkum
- Mads André Skjervik Knutsen
- Emal Mir
- Vetle Mikael With Nygaard
- Isak Øgreid Næss
- Filip Myhren Ormestad

## Status

Under arbeid mot neste leveranse.

- **Forrige leveranse (IS-202):** MVC-app i Docker med skjema, GET/POST og kart. Data ble lagret i minnet.
- **Ferdig implementert siden da:**
  - Lagring i MariaDB med Entity Framework Core, repositories og automatiske migrasjoner
  - Verktøylinje i kartene (Leaflet.Draw) med lagring av radius på behov
  - Påkrevd beskrivelse på både behov og ressurser
  - Koblingsside (`/Kobling`) med kart, prioritetsfarger, oversikt over åpne behov og ledige ressurser, og lagring av koblinger
  - Nytt KartKrise-design med forside, logo, bakgrunn og navigasjonsmeny
  - Enhetstester for controllere og validering (29 tester)
- **Under utvikling:**
  - Innlogging, brukerregistrering og profilmeny – foreløpig kun som designprototyp uten funksjonalitet
  - Autentisering med ASP.NET Core Identity og tilgangskontroll, slik at bare kriseleder har tilgang til koblingssiden

GitHub-lenke: https://github.com/Isak-02/gruppe11-beredskap

## Lisens

Se LICENSE-filen for mer informasjon.
