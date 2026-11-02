# Kriseberedskap - Ressurs- og behovsportal

## Om prosjektet

Dette prosjektet utvikles som en del av IS-20X Case 2026

Målet er å utvikle en webbasert løsning som støtter kriseberedskap og samhandler i Totalforsvaret. Løsningen gjør det mulig for offentlige aktører å registrere behov for bistand, samtidig som privatpersoner, organisasjoner og bedrifter kan registrere tilgjengelige ressurser.

## Funksjonalitet 

- Registrering av behov via skjema (POST) og visning på en egen oversiktsside (GET)
- Registrering av ressurser via skjema (POST) og visning på en egen oversiktsside (GET)
- Kart (Leaflet) med verktøylinje (Leaflet.Draw) på registreringssidene. Posisjonen settes med en markør, og for behov også med en sirkel. Breddegrad, lengdegrad og eventuell radius (meter) lagres sammen med skjemaet, og posisjonen vises i oversikten
- Matching av behov og ressurser på en egen side, der behov og ressurser som ikke allerede er koblet kan kobles sammen
- Validering av påkrevde felt før data lagres
- Responsivt grensesnitt med Bootstrap

## Teknologistack 

- ASP.NET Core MVC (.NET 10)
- C#
- Razor Views
- Docker og Docker Compose
- MariaDB
- Entity Framework Core (Pomelo MySQL-provider)
- Leaflet, Leaflet.Draw og OpenStreetMap
- Bootstrap
- xUnit
- GitHub

## Prosjektstruktur

```text
gruppe11-beredskap/
├── Controllers/     BehovController, RessursController, MatchingController, HomeController
├── DataAccess/      ApplicationDbContext og repositories
├── Migrations/      EF Core-migrasjoner som oppretter og oppdaterer tabellene
├── Models/          BehovViewModel, RessursViewModel, MatchingViewModel, PositionModel
│   └── Entities/    Behov, Ressurs, Matching (tabellene i databasen)
├── Views/           Behov, Ressurs, Matching, Home, Shared
├── wwwroot/         css, js
├── Program.cs
├── Dockerfile
├── compose.yaml     (ligger i rotmappen)
└── gruppe11-beredskap.Tests/   xUnit-tester (ligger i rotmappen)
```

## Installasjon

### Klon repositoriet

```bash
git clone https://github.com/Isak-02/gruppe11-beredskap
```

### Start Docker-containerne

```bash
docker compose up -d
```

### Åpne i nettleser

```text
http://localhost:8080
```

## Systemarkitektur

- Nettleseren sender GET og POST til en ASP.NET Core MVC-controller. Controlleren validerer view-modellen og sender den til et Razor-view.

- BehovController og RessursController har hver en GET-action som viser skjema og kart, en POST-action som lagrer, og en GET-action (`Oversikt`) som viser dataene. Når en markør eller sirkel tegnes, flyttes eller slettes med verktøylinjen i Leaflet-kartet, oppdateres breddegrad, lengdegrad og radius i skjulte felt, og de følger med i POST.

- Data lagres i MariaDB (i Docker) via Entity Framework Core. Repository-klasser i `DataAccess` håndterer lagring og henting, og migrasjonene oppretter og oppdaterer tabellene automatisk når appen starter.

## Testing

Automatiske tester med xUnit, i prosjektet `gruppe11-beredskap.Tests`. Testene bruker falske repositories i minnet, så de kjører uten database. Kjørt i Rider 29. oktober 2026.

| Testklasse | Hva den sjekker | Antall | Resultat |
|---|---|---|---|
| `BehovControllerTests` | GET og POST `Registrer` og `Oversikt` for behov: at gyldige behov lagres, at ugyldige ikke lagres, og at radius lagres og vises riktig | 8 | Bestått |
| `RessursControllerTests` | GET og POST `Registrer` og `Oversikt` for ressurser, og at kjøretøy må ha drivstofftype og lastebiler lastekapasitet | 6 | Bestått |
| `BehovViewModelTests` | Valideringen i `BehovViewModel`: at radius er valgfri, at radius må være større enn 0, og at posisjon på kartet er påkrevd | 5 | Bestått |

Resultat av kjøringen: 19 bestått, 0 feilet, 0 hoppet over.

## Bruk av KI

KI ble brukt som hjelp fra idé til koding. All kode er lest og tilpasset av gruppa.

### Bruksområder

- Forklare MVC-flyten mellom controller, view-modell og view
- Foreslå skjema, validering og hvordan kartklikk kan fylle skjulte felt
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

- Forrige leveranse (IS-202): MVC-app i Docker med skjema, GET/POST og kart. Data ble lagret i minnet.
- Nå: data lagres i MariaDB med Entity Framework Core og migrasjoner, kartene har fått verktøylinje (Leaflet.Draw) med lagring av radius på behov, matching mellom behov og ressurser er lagt til, og det er skrevet enhetstester.

GitHub-lenke: https://github.com/Isak-02/gruppe11-beredskap

## Lisens

Se LICENSE-filen for mer informasjon.