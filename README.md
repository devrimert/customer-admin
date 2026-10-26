# Customer Admin

[![CI](https://github.com/devrimert/customer-admin/actions/workflows/ci.yml/badge.svg)](https://github.com/devrimert/customer-admin/actions/workflows/ci.yml)

**🇬🇧 English version: [scroll down](#english)**

Eine Web-Anwendung für Kunden und ihre Ansprechpartner. Ein Kunde kann viele Ansprechpartner haben (1:N).

- Frontend: React und TypeScript
- Backend: C# mit ASP.NET Core (.NET 10)
- Datenbank: PostgreSQL 18

Weitere Dokumente: [Projektumfang](docs/scope.md) · [Entscheidungen](docs/decisions.md) · [Einsatz von KI](docs/ai-usage.md)

## Starten

### Mit Docker (empfohlen)

Voraussetzung: Docker Desktop und Git.

```bash
git clone https://github.com/devrimert/customer-admin.git
cd customer-admin
cp .env.example .env          # Windows: copy .env.example .env
docker compose up --build
```

| | Adresse |
|---|---|
| Anwendung | http://localhost:8080 |
| API-Dokumentation (Swagger) | http://localhost:8081/swagger |

Beim ersten Start legt die API die Tabellen an und fügt Beispieldaten ein (6 Kunden, 22 Ansprechpartner).
Der erste Build lädt die Images herunter. Das kann ein paar Minuten dauern.

- Stoppen: `docker compose down`. Mit `-v` werden auch die Daten gelöscht.
- Ein Port ist schon belegt? Dann in `.env` den Wert `WEB_PORT`, `API_PORT` oder `POSTGRES_PORT` ändern.

### Lokal (nur die Datenbank in Docker)

Voraussetzung: .NET 10 SDK, Node.js 24 und Docker.

```bash
cp .env.example .env
docker compose up -d db

# einmalig: Connection String mit dem Passwort aus .env
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=customer_admin;Username=customer_admin;Password=<Passwort aus .env>" \
  --project src/CustomerAdmin.Api

cd src/CustomerAdmin.Web
npm ci
npm run dev:all               # startet API und Frontend zusammen
```

| | Adresse |
|---|---|
| Anwendung | http://localhost:5173 |
| Swagger | http://localhost:5261/swagger |

Bereit ist alles, wenn im Log `[api] Now listening on: http://localhost:5261` steht.
Die Ports sind fest eingestellt. Nur wenn Port 5173 schon belegt ist, nimmt Vite den nächsten freien Port und zeigt ihn im Log.

**Das Passwort:** Es ist der Wert von `POSTGRES_PASSWORD` in `.env` (in der Vorlage `change_me`).
Man kann es vor dem ersten `docker compose up` ändern. Danach nicht mehr: PostgreSQL setzt das Passwort nur beim ersten Start.
Zum Ändern später die Datenbank neu anlegen: `docker compose down -v` (löscht die Daten), dann wieder `docker compose up -d db`.

- Mit Beispieldaten: `SeedSampleData=true npm run dev:all`
- Fehler `Connection refused` beim Start? Dann läuft die Datenbank nicht: `docker compose up -d db`

## Funktionen

- Kunden anlegen, bearbeiten, suchen, sortieren, deaktivieren und löschen
- Ansprechpartner pro Kunde anlegen, bearbeiten und löschen
- Ein Ansprechpartner pro Kunde kann Hauptansprechpartner sein. Er steht in der Tabelle immer oben.
- Bearbeiten im Dialog (alle Felder) oder direkt im Feld (ein Feld)
- Die Suche findet Kunden auch über die Namen ihrer Ansprechpartner
- Ändern zwei Personen gleichzeitig denselben Datensatz, wird nichts still überschrieben (`409 Conflict`)
- Warnung beim Verlassen eines Formulars mit ungespeicherten Änderungen
- Oberfläche auf Deutsch, alle Texte in einer Datei

## Tech-Stack

| Bereich | Technik |
|---|---|
| Frontend | React 19, TypeScript, Vite, TanStack Query, React Router, React Hook Form, zod, Pico CSS |
| Backend | ASP.NET Core 10, EF Core 10, Npgsql, JSON Patch, Swagger UI |
| Datenbank | PostgreSQL 18 |
| Tests | xUnit v3, WebApplicationFactory, Testcontainers |
| Betrieb | Docker Compose, nginx, GitHub Actions |

Die TypeScript-Typen für die API werden aus dem OpenAPI-Dokument erzeugt. So passen Frontend und Backend immer zusammen.

## Projektstruktur

```
src/
  CustomerAdmin.Api/        ASP.NET Core API
    Features/Customers/     Controller, Service, Request- und Response-Klassen
    Features/Contacts/
    Domain/                 Entitäten
    Data/                   DbContext, Konfiguration, Migrationen, Beispieldaten
    Common/                 Fehlerbehandlung, Suche, JSON-Patch-Regeln
  CustomerAdmin.Web/        React-Anwendung
    src/features/           Kunden und Ansprechpartner (Seiten, Dialoge, Abfragen, Validierung)
    src/components/         Layout, Dialog, Bearbeiten im Feld, sortierbare Spalten
    src/i18n/de.ts          alle Texte der Oberfläche
tests/
  CustomerAdmin.Api.Tests/  Integrationstests
docs/                       Umfang, Entscheidungen, Einsatz von KI
```

## API

| Methode | Pfad | |
|---|---|---|
| GET | `/api/customers?search=&includeInactive=` | Liste |
| GET | `/api/customers/{id}` | Details mit Ansprechpartnern |
| POST | `/api/customers` | anlegen |
| PUT | `/api/customers/{id}` | alle Felder ändern |
| PATCH | `/api/customers/{id}` | einzelne Felder ändern (JSON Patch) |
| DELETE | `/api/customers/{id}` | löschen, mit allen Ansprechpartnern |
| GET | `/api/customers/{id}/contacts?search=` | Ansprechpartner eines Kunden |
| POST | `/api/customers/{id}/contacts` | Ansprechpartner anlegen |
| GET, PUT, PATCH, DELETE | `/api/customers/{id}/contacts/{contactId}` | |

Fehler kommen als Problem Details (RFC 9457). Zum Ändern schickt man die `version` mit, die man zuletzt gelesen hat.
Hat jemand den Datensatz in der Zwischenzeit geändert, antwortet die API mit `409 Conflict`.

## Tests

```bash
dotnet test                   # Docker muss laufen
```

Die Tests starten die API im Speicher und eine echte PostgreSQL-Datenbank in Docker (Testcontainers).
So prüfen sie auch Dinge, die eine In-Memory-Datenbank nicht kann: den eindeutigen Index für den Hauptansprechpartner,
das kaskadierende Löschen, die Suche ohne Groß- und Kleinschreibung und die Versionsprüfung mit `xmin`.

Beim Schreiben haben die Tests zwei echte Fehler gefunden:
- Die Suche hat `%` und `_` nicht als normalen Text behandelt, weil EF Core `ESCAPE ''` an PostgreSQL geschickt hat.
- Ein `[Produces("application/json")]`-Attribut hat bei Fehlern den Content-Type `application/problem+json` überschrieben.

Bei jedem Pull Request prüft die CI Backend und Frontend: Build (Warnungen gelten als Fehler) und Tests für die API, Lint, Typprüfung und Build für das Frontend.

---

# English

A web app for customers and their contact persons. One customer can have many contacts (1:N).

- Frontend: React and TypeScript
- Backend: C# with ASP.NET Core (.NET 10)
- Database: PostgreSQL 18

More documents: [Scope](docs/scope.md) · [Decisions](docs/decisions.md) · [Use of AI](docs/ai-usage.md)

## Getting started

### With Docker (recommended)

Requirements: Docker Desktop and Git.

```bash
git clone https://github.com/devrimert/customer-admin.git
cd customer-admin
cp .env.example .env          # Windows: copy .env.example .env
docker compose up --build
```

| | URL |
|---|---|
| Web app | http://localhost:8080 |
| API docs (Swagger) | http://localhost:8081/swagger |

On first start the API creates the tables and adds sample data (6 customers, 22 contacts).
The first build downloads the base images, which can take a few minutes.

- Stop: `docker compose down`. Add `-v` to also delete the data.
- A port is already in use? Change `WEB_PORT`, `API_PORT` or `POSTGRES_PORT` in `.env`.

### Locally (only the database in Docker)

Requirements: .NET 10 SDK, Node.js 24 and Docker.

```bash
cp .env.example .env
docker compose up -d db

# once: connection string with the password from .env
dotnet user-secrets set "ConnectionStrings:Default" \
  "Host=localhost;Port=5432;Database=customer_admin;Username=customer_admin;Password=<password from .env>" \
  --project src/CustomerAdmin.Api

cd src/CustomerAdmin.Web
npm ci
npm run dev:all               # starts the API and the frontend together
```

| | URL |
|---|---|
| Web app | http://localhost:5173 |
| Swagger | http://localhost:5261/swagger |

Everything is ready when the log shows `[api] Now listening on: http://localhost:5261`.
The ports are fixed. Only if port 5173 is already in use, Vite picks the next free port and shows it in the log.

**The password:** it is the value of `POSTGRES_PASSWORD` in `.env` (`change_me` in the template).
You can change it before the first `docker compose up`. Not after that: PostgreSQL sets the password only on its first start.
To change it later, recreate the database: `docker compose down -v` (deletes the data), then `docker compose up -d db` again.

- With sample data: `SeedSampleData=true npm run dev:all`
- Error `Connection refused` on start? The database is not running: `docker compose up -d db`

## Features

- Create, edit, search, sort, deactivate and delete customers
- Create, edit and delete contacts per customer
- One contact per customer can be the primary contact. It always stays on top of the table.
- Edit in a dialog (all fields) or inline (one field)
- The search also finds customers by the names of their contacts
- If two people change the same record at the same time, nothing is overwritten silently (`409 Conflict`)
- Warning before leaving a form with unsaved changes
- German UI, all texts in one file

## Tech stack

| Area | Technology |
|---|---|
| Frontend | React 19, TypeScript, Vite, TanStack Query, React Router, React Hook Form, zod, Pico CSS |
| Backend | ASP.NET Core 10, EF Core 10, Npgsql, JSON Patch, Swagger UI |
| Database | PostgreSQL 18 |
| Tests | xUnit v3, WebApplicationFactory, Testcontainers |
| Delivery | Docker Compose, nginx, GitHub Actions |

The TypeScript types for the API are generated from the OpenAPI document, so frontend and backend always match.

## Project structure

```
src/
  CustomerAdmin.Api/        ASP.NET Core API
    Features/Customers/     controller, service, request and response contracts
    Features/Contacts/
    Domain/                 entities
    Data/                   DbContext, configuration, migrations, sample data
    Common/                 error handling, search, JSON Patch rules
  CustomerAdmin.Web/        React app
    src/features/           customers and contacts (pages, dialogs, queries, validation)
    src/components/         layout, dialog, inline editing, sortable columns
    src/i18n/de.ts          all UI texts
tests/
  CustomerAdmin.Api.Tests/  integration tests
docs/                       scope, decisions, use of AI
```

## API

| Method | Path | |
|---|---|---|
| GET | `/api/customers?search=&includeInactive=` | list |
| GET | `/api/customers/{id}` | details with contacts |
| POST | `/api/customers` | create |
| PUT | `/api/customers/{id}` | change all fields |
| PATCH | `/api/customers/{id}` | change single fields (JSON Patch) |
| DELETE | `/api/customers/{id}` | delete, with all contacts |
| GET | `/api/customers/{id}/contacts?search=` | contacts of a customer |
| POST | `/api/customers/{id}/contacts` | create a contact |
| GET, PUT, PATCH, DELETE | `/api/customers/{id}/contacts/{contactId}` | |

Errors are returned as problem details (RFC 9457). Updates send the `version` from the last read.
If someone changed the record in the meantime, the API answers `409 Conflict`.

## Tests

```bash
dotnet test                   # Docker must be running
```

The tests start the API in memory and a real PostgreSQL database in Docker (Testcontainers).
So they also check what an in-memory database can't: the unique index for the primary contact,
cascade delete, case-insensitive search and the version check with `xmin`.

While they were written, the tests found two real bugs:
- The search did not treat `%` and `_` as plain text, because EF Core sent `ESCAPE ''` to PostgreSQL.
- A `[Produces("application/json")]` attribute replaced the `application/problem+json` content type of error responses.

On every pull request, CI checks backend and frontend: build (warnings as errors) and tests for the API, lint, type check and build for the frontend.
