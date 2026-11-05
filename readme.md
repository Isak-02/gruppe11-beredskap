# KartKrise – Ressurs- og behovsportal

## Om prosjektet

Dette prosjektet utvikles som en del av IS-20X Case 2026.

KartKrise er en webløsning for kriseberedskap i Totalforsvaret. Offentlige aktører kan registrere behov for hjelp, og privatpersoner, organisasjoner og bedrifter kan registrere ressurser de har tilgjengelig. Behov og ressurser vises i kart, og kan kobles sammen.

## Funksjonalitet

- Registrering av behov og ressurser via skjema (GET/POST), med egne oversiktssider
- Kart (Leaflet med Leaflet.Draw) der posisjon settes med markør, og for behov også med sirkel (radius)
- Kobling av behov og ressurser på siden `/Kobling`, med kart og oversikt over åpne behov og ledige ressurser
- Validering av påkrevde felt før data lagres
- Lagring i MariaDB med Entity Framework Core
- Eget KartKrise-design med Bootstrap
- Registrering lagrer brukeren i AspNetUsers
- Innlogging bruker ASP.NET Identity
- `/Kobling` krever rollen Koordinator

## Teknologistack

- ASP.NET Core MVC (.NET 10) og C#
- Razor Views
- Entity Framework Core (Pomelo MySQL-provider)
- MariaDB
- Docker og Docker Compose
- Leaflet, Leaflet.Draw og OpenStreetMap
- Bootstrap
- xUnit
- JetBrains Rider og GitHub

## Prosjektstruktur

```text
gruppe11-beredskap/
├── Properties/                    launchSettings.json (kjøreprofiler for Rider)
├── wwwroot/                       css, js, bilder
├── Controllers/                   AccountController, BehovController, RessursController, MatchingController, HomeController
├── DataAccess/                    ApplicationDbContext og repositories
├── Migrations/                    EF Core-migrasjoner
├── Models/                        BehovViewModel, RessursViewModel, MatchingViewModel
│   ├── Authorization/             Rollene KriseInnmelder, RessursInnmelder og Koordinator
│   └── Entities/                  Behov, Ressurs, Matching, Bruker (tabellene i databasen)
├── Views/                         Account, Behov, Ressurs, Matching, Home, Shared
├── appsettings.json
├── appsettings.Development.json
├── Dockerfile
└── Program.cs

gruppe11-beredskap.Tests/          xUnit-tester

compose.yaml                       (ligger i rotmappen)
```

## Installasjon

### Klon repositoriet

```bash
git clone https://github.com/Isak-02/gruppe11-beredskap
```

### Alternativ 1: Docker Compose

```bash
docker compose up -d --build
```

Åpne http://localhost:8080 i nettleseren.

### Alternativ 2: Run-knappen i JetBrains Rider

1. Start databasen: `docker compose up -d db`
2. Åpne `gruppe11-beredskap.sln` i Rider
3. Trykk på **Run** (▶). Appen åpnes på http://localhost:5178

Tabellene i databasen opprettes automatisk når appen starter.

## Systemarkitektur

- Nettleseren sender GET og POST til en controller. Controlleren validerer view-modellen og sender den til et Razor-view.
- Når man tegner på kartet, lagres posisjon (og eventuell radius) i skjulte felt som sendes med skjemaet.
- Data lagres i MariaDB via Entity Framework Core og repository-klasser i `DataAccess`.
- Innlogging bruker en cookie fra ASP.NET Identity, med rollene KriseInnmelder, RessursInnmelder og Koordinator, og en ressurs kan registreres, endres og slettes av eieren eller en koordinator.

## Testing

Enhetstester med xUnit ligger i `gruppe11-beredskap.Tests`. Testene bruker falske repositories, så de trenger ingen database. Kjør dem i Rider eller med `dotnet test`.

| Testklasse | Hva den sjekker | Antall |
|---|---|---|
| `BehovControllerTests` | Registrering og oversikt av behov, inkludert radius og beskrivelse | 13 |
| `RessursControllerTests` | Registrering, endring, sletting og oversikt av ressurser, krav om drivstoff og lastekapasitet | 10 |
| `BehovViewModelTests` | Validering av radius, posisjon og beskrivelse | 8 |
| `MatchingControllerTests` | Visning av åpne behov og ledige ressurser, og lagring av koblinger | 5 |
| `TilgangTests` | Rollekrav på registrering, oversikt og kobling | 7 |

Siste kjøring (5. november 2026): 43 bestått, 0 feilet.

### Systemtest

Resultat: Ikke kjørt ennå

- Registrer bruker
- Logg inn
- Registrer ressurs, endre den og slett den

### Sikkerhetstest

Resultat: Ikke kjørt ennå

- Utlogget bruker kommer ikke inn på endring
- Vanlig bruker kommer ikke inn på `/Kobling`
- Passord i AspNetUsers er en hash

### Brukervennlighetstest

Resultat: Ikke kjørt ennå

- Registrer ressurs på smal skjerm
- Sett markør i kartet
- Les oversikten

## Bruk av KI

KI ble brukt som hjelp fra idé til koding. All kode er lest og tilpasset av gruppa.

### Bruksområder

- Forklare MVC-flyten mellom controller, view-modell og view
- Foreslå skjema, validering og hvordan kartklikk kan fylle skjulte felt
- Hjelp med database, tester og dokumentasjon
- Finne mangler i oppgaven
- Få tilbakemeldinger
- Opprette klasser

### Verktøy

- JetBrains Rider med AI Assistant, Claude, ChatGPT, Copilot

### Eksempler på prompt-kommandoer

- «Hva mangler vi mot disse kravene: controller, view model, view, GET/POST, skjema, kart, Docker og dokumentasjon?»
- «Hvordan kan vi løse dette?: Når man velger posisjon i kart, og velger en ny posisjon skal forrige posisjon bli slettet»
- «Hvorfor får jeg ikke til å pushe til branch? Her er feilmeldingene: X»
- «Gi en tilbakemelding på filstruktur, og kom med forslag til hvordan den kan forbedres.»

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
- **Ferdig nå:** Lagring i MariaDB, kart med verktøylinje og radius, beskrivelse på behov og ressurser, koblingsside, innlogging med roller, nytt design og 43 enhetstester.

GitHub-lenke: https://github.com/Isak-02/gruppe11-beredskap

## Lisens

Se LICENSE-filen for mer informasjon.
