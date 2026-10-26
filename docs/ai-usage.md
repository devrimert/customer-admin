# Einsatz von KI

**🇬🇧 English version: [scroll down](#use-of-ai)**

Ich habe in allen Bereichen mit Claude Code und Claude Desktop gearbeitet, als Pair Programmer und KI-Assistent.
Die Architektur und die Entscheidungen kommen von mir. Jede Änderung habe ich geprüft und getestet.

| Anteil der KI | Bereiche |
|---|---|
| Komplett von der KI | Design der SVG-Icons |
| Mit viel Hilfe der KI | Tests, Frontend-Design, Docker, CI, Dokumentation, Übersetzung |

## So habe ich gearbeitet

- Zuerst Plan und Optionen besprechen, dann entscheide ich, dann wird umgesetzt.
- Kleine Schritte: ein Thema pro Branch und Pull Request.
- Jede Änderung lese ich, baue sie und probiere sie aus.

## Wo ich korrigiert oder entschieden habe

- **Passwort im Repository:** Die KI hat den Connection String mit Passwort in `appsettings.Development.json` geschrieben. Ich habe das abgelehnt, weil Secrets nur in `.env` und User Secrets gehören.
- **Paketversionen:** Die KI hat zuerst die neuesten Hauptversionen gewählt. Ich habe festgelegt: LTS für Plattformen, stabile und verbreitete Versionen für Bibliotheken.
- **Concurrency:** Ich wollte keinen einfachen "Last write wins". Daraus wurde die Versionsprüfung mit `xmin` und `409 Conflict`.
- **JSON Patch:** Für Änderungen an einzelnen Feldern wollte ich einen Standard (RFC 6902) statt eigener Endpunkte.
- **Kunden löschen:** Wegen der DSGVO habe ich entschieden, dass man Kunden deaktivieren und auch löschen kann.
- **Oberfläche:** Den ersten Entwurf fand ich zu schwach. Ich habe Layout, Farben und Verhalten vorgegeben (Sidebar, Tabellen, Stift nur bei Hover).
- **Name bearbeiten:** Die KI hat Vor- und Nachname als zwei getrennte Felder mit zwei Stiften gebaut. Ich wollte einen Stift, der beide Felder öffnet. Jetzt geht beides in einer Anfrage.
- **Detailseite:** Ein Vorschlag mit zwei Spalten hat Label und Wert schwer lesbar gemacht. Ich habe ihn zurückgenommen.
- **Umfang:** Ein Dashboard war meine Idee. Gemeinsam haben wir entschieden, es nicht zu bauen, weil die Aufgabe es nicht braucht.

---

# Use of AI

I worked with Claude Code and Claude Desktop in all areas, as a pair programmer and AI assistant.
The architecture and the decisions are mine. I reviewed and tested every change.

| Share of AI | Areas |
|---|---|
| Fully done by AI | Design of the SVG icons |
| With a lot of help from AI | Tests, frontend design, Docker, CI, documentation, translation |

## How I worked

- First discuss the plan and the options, then I decide, then it gets built.
- Small steps: one topic per branch and pull request.
- I read, build and try out every change.

## Where I corrected or decided

- **Password in the repository:** The AI put the connection string with a password into `appsettings.Development.json`. I rejected it, because secrets belong only in `.env` and user secrets.
- **Package versions:** The AI first picked the newest major versions. I set the rule: LTS for platforms, stable and widely used versions for libraries.
- **Concurrency:** I didn't want a simple "last write wins". This led to the version check with `xmin` and `409 Conflict`.
- **JSON Patch:** For changing single fields I wanted a standard (RFC 6902) instead of custom endpoints.
- **Deleting customers:** Because of the GDPR I decided that customers can be deactivated and also deleted.
- **UI:** I found the first draft too weak. I gave the layout, colors and behavior (sidebar, tables, pencil only on hover).
- **Editing the name:** The AI built first and last name as two separate fields with two pencils. I wanted one pencil that opens both fields. Now both are saved in one request.
- **Detail page:** A two-column layout made labels and values hard to read. I reverted it.
- **Scope:** A dashboard was my idea. Together we decided not to build it, because the task doesn't need it.
