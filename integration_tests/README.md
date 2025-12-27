# FireInvent k6 Load Testing

Diese Umgebung startet k6 mit InfluxDB und Grafana, um die FireInvent-API unter Last zu testen. Der k6-Test erzeugt bewusst viele neue Daten (Departments, Persons, Manufacturers, ProductTypes, Products, Variants, StorageLocations, Items, Appointments, Visits, VisitItems), damit auch Schreibpfade gestresst werden.

## Dateien
- `docker-compose.yml`: Stack mit InfluxDB, Grafana und k6.
- `scripts/k6-load-test.js`: k6-Szenario mit schreiblastiger Last.
- `grafana/provisioning/*`: Automatische Grafana-Provisionierung (Datasource + Dashboard).

## Voraussetzungen
- Docker + Docker Compose
- OAuth2 Client Credentials gegen Keycloak unter `auth.fireinvent.de` (oder fertiger Bearer Token als Fallback)
   - `K6_OAUTH_TOKEN_URL` (z. B. `https://auth.fireinvent.de/realms/<realm>/protocol/openid-connect/token`)
   - `K6_CLIENT_ID`
   - `K6_CLIENT_SECRET`
   - optional: `K6_SCOPE`

## Start
1. Stack starten (InfluxDB + Grafana):
   ```bash
   docker compose up -d influxdb grafana
   ```
2. Lasttest ausführen (k6 als Einmal-Run, holt Token via Client Credentials):
   ```bash
   docker compose run --rm \
     -e K6_BASE_URL="http://api.fireinvent.de" \
       -e K6_OAUTH_TOKEN_URL="https://auth.fireinvent.de/realms/<realm>/protocol/openid-connect/token" \
       -e K6_CLIENT_ID="<client-id>" \
       -e K6_CLIENT_SECRET="<client-secret>" \
       -e K6_SCOPE="<optional scopes>" \
     -e K6_RATE=25 \   # Anfragen pro Sekunde (optional)
     -e K6_DURATION=10m \   # Laufzeit (optional)
     -e K6_PREALLOCATED_VUS=50 \   # Initiale VUs (optional)
     -e K6_MAX_VUS=200 \   # Obergrenze VUs (optional)
     k6
   ```
   Der Container nutzt `scripts/k6-load-test.js` und schreibt alle Metriken nach InfluxDB.

3. Grafana öffnen: http://localhost:3000 (admin / admin). Das Dashboard „k6 Overview“ ist vorkonfiguriert.

## Was der Test macht
- Erstellt für jeden virtuellen User neue Entities mit eindeutigen Namen (UUID-Suffix), um Kollisionen zu vermeiden.
- Schreibt: Department → Person → Manufacturer → ProductType → Product → Variant → StorageLocation → Item → Appointment → Visit → VisitItem.
- Führt nach den POSTs einige GETs aus, um die frisch erstellten Ressourcen zu lesen.
- Thresholds: `http_req_failed < 2%`, `http_req_duration p95 < 1500ms`.

## Aufräumen
```bash
docker compose down -v
```

## Hinweise
- Alternativ kannst du weiterhin einen festen Token per `K6_TOKEN` setzen; bei gesetzter ClientId/Secret wird jedoch automatisch OAuth2 genutzt und der Token pro VU zwischengespeichert.
- Wenn ein Endpoint andere Pflichtfelder oder Status-Codes nutzt, kannst du die Payloads in `scripts/k6-load-test.js` anpassen.
- Für höhere Last einfach `K6_RATE`, `K6_DURATION` oder `K6_MAX_VUS` anpassen.
