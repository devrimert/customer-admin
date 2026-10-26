# Entscheidungen

**🇬🇧 English version: [scroll down](#decisions)**

Hier stehen nur die wichtigen Entscheidungen, also die, die man auch anders hätte treffen können.

### 1. Ordner nach Features, keine Repository-Schicht

Der Code ist nach Features sortiert (`Features/Customers`, `Features/Contacts`), nicht nach Technik (`Controllers`, `Services`).
Eine Änderung an einem Feature bleibt meistens in einem Ordner.
Die Services nutzen den `DbContext` direkt. EF Core ist schon Unit of Work und Repository, eine weitere Schicht bringt hier keinen Vorteil.
Auch Interfaces für die Services gibt es nicht: Die Tests laufen gegen eine echte Datenbank, es gibt nichts zu mocken.

### 2. Schutz vor parallelen Änderungen mit `xmin`

Jeder Datensatz hat eine `version`. Sie ist die PostgreSQL-Systemspalte `xmin`, die sich bei jeder Änderung ändert. Eine eigene Spalte ist nicht nötig.
Beim Speichern prüft die Datenbank die Version. Passt sie nicht, antwortet die API mit `409 Conflict` und die Oberfläche zeigt den aktuellen Stand.
So überschreibt niemand still die Änderung von jemand anderem.

### 3. PUT für das Formular, JSON Patch für einzelne Felder

Das Formular schickt alle Felder mit `PUT`. Das Bearbeiten direkt im Feld schickt nur ein Feld mit `PATCH` (JSON Patch, RFC 6902).
Die erste Operation muss `test /version` sein, sonst lehnt die API den Patch ab.
Beide Wege nutzen dieselbe Validierung und dieselbe Service-Methode.

### 4. Höchstens ein Hauptansprechpartner, geprüft von der Datenbank

Ein Partial Unique Index (`WHERE is_primary`) sorgt dafür, dass ein Kunde nur einen Hauptansprechpartner hat.
Eine Prüfung nur im C#-Code hätte eine Race Condition bei zwei gleichzeitigen Anfragen.

### 5. Kunden deaktivieren oder löschen (DSGVO)

Deaktivieren ist der normale Weg und kann rückgängig gemacht werden.
Ansprechpartner sind Personen, ihre Daten sind personenbezogen. Deshalb kann man sie löschen, und beim Löschen eines Kunden werden sie mitgelöscht.

### 6. Tests gegen eine echte PostgreSQL

Die Integrationstests nutzen Testcontainers. So werden auch der Index, das kaskadierende Löschen und die Suche mit `ILIKE` getestet.
Dafür braucht man Docker, um die Tests auszuführen.

### 7. Sortieren im Browser, kein Paging

Die API gibt die ganze Liste zurück, der Browser sortiert sie. Das reicht für einige tausend Datensätze.
Für mehr Daten wären Paging und Sortierung in der API der nächste Schritt.

### 8. Konfiguration, Secrets und Docker

Passwörter stehen nicht im Repository. Docker Compose liest sie aus `.env`, lokal kann man User Secrets nutzen.
Die API führt die Migrationen beim Start aus, deshalb reicht `docker compose up`.
nginx liefert das Frontend aus und leitet `/api` an die API weiter, so braucht man kein CORS.

---

# Decisions

Only the important decisions are listed here, the ones that could reasonably have gone another way.

### 1. Feature folders, no repository layer

The code is grouped by feature (`Features/Customers`, `Features/Contacts`), not by technical type (`Controllers`, `Services`).
A change to a feature usually stays in one folder.
Services use the `DbContext` directly. EF Core already is unit of work and repository, another layer adds nothing here.
There are no service interfaces either: tests run against a real database, so there is nothing to mock.

### 2. Protection against concurrent changes with `xmin`

Every record has a `version`. It is PostgreSQL's system column `xmin`, which changes on every update. No extra column is needed.
On save, the database checks the version. If it doesn't match, the API answers `409 Conflict` and the UI shows the current state.
So nobody silently overwrites someone else's change.

### 3. PUT for the form, JSON Patch for single fields

The form sends all fields with `PUT`. Inline editing sends only one field with `PATCH` (JSON Patch, RFC 6902).
The first operation must be `test /version`, otherwise the API rejects the patch.
Both use the same validation and the same service method.

### 4. At most one primary contact, enforced by the database

A partial unique index (`WHERE is_primary`) makes sure a customer has only one primary contact.
A check in C# alone would have a race condition with two requests at the same time.

### 5. Deactivate or delete customers (GDPR)

Deactivating is the normal way and can be undone.
Contacts are people, so their data is personal data. That's why they can be deleted, and they are deleted together with their customer.

### 6. Tests against a real PostgreSQL

The integration tests use Testcontainers. So the index, cascade delete and the `ILIKE` search are tested as well.
The trade-off: Docker is needed to run the tests.

### 7. Sorting in the browser, no paging

The API returns the whole list and the browser sorts it. This is enough for a few thousand records.
For more data, paging and sorting in the API would be the next step.

### 8. Configuration, secrets and Docker

Passwords are not in the repository. Docker Compose reads them from `.env`, locally user secrets can be used.
The API runs the migrations on startup, so `docker compose up` is enough.
nginx serves the frontend and forwards `/api` to the API, so no CORS is needed.
