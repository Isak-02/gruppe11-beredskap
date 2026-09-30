# Kriseberedskap - Ressurs- og behovsportal

## Om prosjektet

Dette prosjektet utvikles som en del av IS-20X Case 2026

Målet er å utvikle en webbasert løsning som støtter kriseberedskap og samhandler i Totalforsvaret. Løsningen gjør det mulig for offentlige aktører å registrere behov for bistand, samtidig som privatpersoner, organisasjoner og bedrifter kan registrere tilgjengelige ressurser.

## Funksjonalitet 

- Registrering av behov via skjema (POST) og visning på en egen oversiktsside (GET)
- Registrering av ressurser via skjema (POST) og visning på en egen oversiktsside (GET)
- Kart (Leaflet) på registreringssidene. Klikk i kartet fyller inn breddegrad og lengdegrad, som lagres sammen med skjemaet og vises i oversikten
- Validering av påkrevde felt før data lagres
- Responsivt grensesnitt med Bootstrap

## Teknologistack 

- ASP.NET Core MVC (.NET 10)
- C#
- Razor Views
- Docker og Docker Compose
- Leaflet og OpenStreetMap
- Bootstrap
- GitHub

## Prosjektstruktur

```text
gruppe11-beredskap/
├── Controllers/     BehovController, RessursController, HomeController
├── Models/          BehovViewModel, RessursViewModel, PositionModel
├── Views/           Behov, Ressurs, Home, Shared
├── wwwroot/         css, js
├── Program.cs
├── Dockerfile
└── compose.yaml     (ligger i rotmappen)
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

- BehovController og RessursController har hver en GET-action som viser skjema og kart, en POST-action som lagrer, og en GET-action (`Oversikt`) som viser dataene. Et klikk i Leaflet-kartet skriver breddegrad og lengdegrad inn i skjulte felt, og de følger med i POST.

- Data lagres i lister i minnet og forsvinner når appen starter på nytt. Det er ingen database.

## Testing

Automatiske tester med xUnit, i prosjektet `gruppe11-beredskap.Tests`. Kjørt med `dotnet test` 30. september 2026.

| Test | Hva den sjekker | Resultat |

| `Index_ReturnsViewResult` | Forsiden (`Home/Index`) returnerer en side | Bestått |
| `CorrectMap_Get_ReturnsViewResult` | Kartskjemaet (`CorrectMap`) vises når siden åpnes | Bestått |
| `CorrectMap_Post_WithValidModel_ReturnsCorrectionOverview` | Når skjemaet sendes inn med breddegrad, lengdegrad og beskrivelse, vises siden `CorrectionOverview` | Bestått |

Resultat av kjøringen: 3 bestått, 0 feilet, 0 hoppet over.

## Bruk av KI

KI ble brukt som hjelp fra idé til koding. All kode er lest og tilpasset av gruppa.

### Bruksområder

- Forklare MVC-flyten mellom controller, view-modell og view
- Foreslå skjema, validering og hvordan kartklikk kan fylle skjulte felt
- Finne mangler i oppgaven
- Få tilbakemeldinger
- Opprette klasser

### Verktøy

- JetBrains Rider med AI Assistant, modell Claude, Chatgpt, Copilot

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

Leveranse klar for IS-202: MVC-app i Docker med skjema, GET/POST og kart. Data lagres i minnet. GitHub-lenke: https://github.com/Isak-02/gruppe11-beredskap

## Lisens

Se LICENSE-filen for mer informasjon.