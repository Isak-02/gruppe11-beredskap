# Kriseberedskap - Ressurs- og behovsportal

## Om prosjektet

Dette prosjektet utvikles som en del av IS-20X Case 2026

Målet er å utvikle en webbasert løsning som støtter kriseberedskap og samhandler i Totalforsvaret. Løsningen gjør det mulig for offentlige aktører å registere behov for bistand, samtidig som privatpersoner, organisasjoner og bedrifter kan registere tilgjengelige ressurser.

## Funksjonalitet 

- Registrering av behov
- Registrering av ressurser
- Rollebasert tilgangskontroll
- Kartvisning med Leaflet
- Matching mellom behov og ressurser
- Statushåndtering av oppdrag

## Teknologistack 

 - ASP.NET Core MVC
 - C#
 - Entity Framework Core
 - SQL Server
 - Docker Compose
 - Leaflet (kartvisning)
 - Github
 - JetBrains Rider

## Prosjektstruktur

```text
gruppe11-beredskap-portal
│
├── Controllers
├── Models
├── Views
├── wwwroot
├── Program.cs
├── appsettings.json
├── compose.yaml
```

## Installasjon

### Klon repositoriet

```bash
git clone <repository-url>
```

### Start Docker-containerne

```bash
docker compose up -d
```

### Kjør applikasjonen

```bash
dotnet run
```

### Åpne i nettleser

```text
https;//localhost:5001
```

## Team

Gruppe 11

- Navn 1
- Navn 2
- Navn 3
- Navn 4

## Status

Prosjektet er under utvikling.

## Lisens

Se LICENSE-filen for mer informasjon.