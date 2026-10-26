# Projektumfang

**🇬🇧 English version: [scroll down](#scope)**

## Ziel

Eine kleine, aber vollständige Anwendung: Kunden und ihre Ansprechpartner verwalten.

Das Projekt soll zeigen, wie ich in vielen Bereichen arbeite: API-Design, Datenbank, Frontend, Tests, Docker und CI.
Es sollte nicht zu groß werden. Aber alles, was drin ist, sollte eine hohe Qualität haben.

Aufwand: ungefähr 12 Stunden.

## Im Umfang

- Kunden: anlegen, bearbeiten, suchen, sortieren, deaktivieren, löschen
- Ansprechpartner: anlegen, bearbeiten, löschen, Hauptansprechpartner festlegen
- REST-API mit OpenAPI-Dokumentation, Fehlern nach RFC 9457 und Schutz vor parallelen Änderungen
- Deutsche Oberfläche mit Bearbeiten im Dialog und direkt im Feld
- Integrationstests gegen eine echte PostgreSQL-Datenbank
- Start mit einem Befehl über Docker Compose, mit Beispieldaten
- CI mit GitHub Actions

## Bewusst nicht im Umfang

| Thema | Warum nicht | Nächster Schritt |
|---|---|---|
| Login und Rollen | Für eine kleine Verwaltung zu viel. Ohne Login gibt es auch kein `CreatedBy` / `UpdatedBy`. | ASP.NET Core Identity oder ein externer Anbieter (z. B. Entra ID) |
| Seiten (Paging) | Die API gibt die ganze Liste zurück. Ein Test mit 1.000 Kunden war noch schnell, aber die Seite wird lang. | Paging und Sortierung in der API, Trigram-Index für die Suche |
| Mehrere Telefonnummern oder Adressen | Eine Nummer und eine Adresse reichen für den Kern. | Eigene Tabellen mit Typ (z. B. mobil, Büro) |
| Zweite Sprache, dunkles Design | Nicht gefordert. | Vorbereitet: alle Texte in einer Datei, `data-theme` |
| Frontend-Tests | Die wichtigen Regeln liegen im Backend und sind dort getestet. | Vitest und Testing Library |
| Produktiver Betrieb | Docker Compose ist für die lokale Demo gedacht. | Production-Modus, Secrets aus einem Secret Store, Migrationen als eigener Schritt |

---

# Scope

## Goal

A small but complete application: manage customers and their contact persons.

The project should show how I work across many areas: API design, database, frontend, tests, Docker and CI.
It should not get too big. But everything that is in it should be of high quality.

Effort: about 12 hours.

## In scope

- Customers: create, edit, search, sort, deactivate, delete
- Contacts: create, edit, delete, set the primary contact
- REST API with OpenAPI docs, errors as RFC 9457 problem details and protection against concurrent changes
- German UI with editing in a dialog and inline
- Integration tests against a real PostgreSQL database
- One-command start with Docker Compose, with sample data
- CI with GitHub Actions

## Out of scope (on purpose)

| Topic | Why not | Next step |
|---|---|---|
| Login and roles | Too much for a small admin tool. Without a login there is also no `CreatedBy` / `UpdatedBy`. | ASP.NET Core Identity or an external provider (e.g. Entra ID) |
| Paging | The API returns the whole list. A test with 1,000 customers was still fast, but the page gets long. | Paging and sorting in the API, trigram index for the search |
| Several phone numbers or addresses | One number and one address are enough for the core. | Own tables with a type (e.g. mobile, office) |
| Second language, dark theme | Not required. | Prepared: all texts in one file, `data-theme` |
| Frontend tests | The important rules live in the backend and are tested there. | Vitest and Testing Library |
| Production hosting | Docker Compose is meant for a local demo. | Production mode, secrets from a secret store, migrations as a separate step |
