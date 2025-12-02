# Keycloak Production Deployment Konzept

## Übersicht

Dieses Dokument beschreibt das Konzept für den produktiven Einsatz von Keycloak mit FireInvent. In der Entwicklung wird `start-dev` verwendet, was für Produktion nicht geeignet ist.

## Aktueller Stand (Entwicklung)

- Keycloak läuft mit `start-dev` Kommando
- Kein HTTPS erforderlich
- Vereinfachte Konfiguration
- Geeignet für lokale Entwicklung und Tests

## Produktions-Anforderungen

### 1. Keycloak Production Mode

Keycloak muss mit dem `start` Kommando (statt `start-dev`) gestartet werden:

```yaml
command: ["start"]
```

### 2. HTTPS/TLS Konfiguration

Keycloak erfordert in Produktion HTTPS. Es gibt zwei empfohlene Ansätze:

#### Option A: Reverse Proxy (Empfohlen)

Ein Reverse Proxy (nginx, Traefik, Caddy) terminiert TLS:

```
Internet → Reverse Proxy (TLS) → Keycloak (HTTP)
```

Konfiguration in docker-compose.prod.yml:
```yaml
environment:
  KC_PROXY_HEADERS: xforwarded
  KC_HTTP_ENABLED: "true"
  KC_HOSTNAME_STRICT_HTTPS: "false"
```

#### Option B: Direktes TLS auf Keycloak

Keycloak handhabt TLS direkt:

```yaml
environment:
  KC_HTTPS_CERTIFICATE_FILE: /opt/keycloak/conf/server.crt
  KC_HTTPS_CERTIFICATE_KEY_FILE: /opt/keycloak/conf/server.key
volumes:
  - ./certs/server.crt:/opt/keycloak/conf/server.crt:ro
  - ./certs/server.key:/opt/keycloak/conf/server.key:ro
```

### 3. Hostname Konfiguration

Der externe Hostname muss korrekt konfiguriert sein:

```yaml
environment:
  KC_HOSTNAME: auth.example.com
```

### 4. Datenbank

PostgreSQL wird bereits verwendet - keine Änderung nötig. Für Produktion empfohlen:

- Regelmäßige Backups der Datenbank
- Überwachung der Datenbankperformance
- Ggf. Connection Pool Optimierung

## Migrations-Schritte

### Schritt 1: Infrastruktur vorbereiten

1. DNS-Eintrag für Keycloak erstellen (z.B. `auth.example.com`)
2. TLS-Zertifikate beschaffen (Let's Encrypt empfohlen)
3. Reverse Proxy konfigurieren (falls verwendet)

### Schritt 2: Keycloak-Konfiguration

1. `.env` Datei von `.env.example` kopieren und anpassen
2. Production-Werte für `KEYCLOAK_HOSTNAME` setzen
3. Sichere Passwörter für alle Credentials verwenden

### Schritt 3: Deployment

```bash
# Mit Production Overlay starten
docker compose -f docker-compose.yml -f docker-compose.prod.yml up -d
```

### Schritt 4: Validierung

1. Health-Endpoint prüfen: `https://auth.example.com/health/ready`
2. Admin-Console testen: `https://auth.example.com/admin/`
3. OIDC Discovery testen: `https://auth.example.com/realms/fireinvent/.well-known/openid-configuration`

## Beispiel: Nginx Reverse Proxy

```nginx
server {
    listen 443 ssl http2;
    server_name auth.example.com;

    ssl_certificate /etc/letsencrypt/live/auth.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/auth.example.com/privkey.pem;

    location / {
        proxy_pass http://keycloak:8080;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_set_header X-Forwarded-Host $host;
        proxy_buffer_size 128k;
        proxy_buffers 4 256k;
        proxy_busy_buffers_size 256k;
    }
}
```

## Sicherheitshinweise

1. **Admin-Passwort**: Nach dem ersten Login das Admin-Passwort ändern
2. **Brute-Force Protection**: In Keycloak aktivieren (standardmäßig aktiv)
3. **Session-Timeouts**: Angemessene Werte konfigurieren
4. **Audit Logging**: Aktivieren für Compliance

## FireInvent API Konfiguration

Die API muss auf den produktiven Keycloak zeigen:

```env
FIREINVENT_AUTHORITY=https://auth.example.com/realms/fireinvent
FIREINVENT_OIDC_DISCOVERY_URL_FOR_SWAGGER=https://auth.example.com/realms/fireinvent/.well-known/openid-configuration
AUTH_REQUIRE_HTTPS_METADATA=true
```

## Monitoring

Keycloak Metrics können aktiviert werden:

```yaml
environment:
  KC_METRICS_ENABLED: "true"
```

Metrics sind dann unter `/metrics` verfügbar und können von Prometheus gesammelt werden.

## Backup-Strategie

1. **Datenbank**: Regelmäßige PostgreSQL Dumps
2. **Realm-Export**: Keycloak Realm als JSON exportieren (Admin Console oder CLI)

```bash
# Realm Export via CLI
docker exec fireinvent_keycloak /opt/keycloak/bin/kc.sh export --realm fireinvent --dir /tmp/export
```

## Zusammenfassung

| Aspekt | Entwicklung | Produktion |
|--------|-------------|------------|
| Kommando | `start-dev` | `start` |
| TLS | Nicht erforderlich | Erforderlich |
| Hostname | localhost | Konfiguriert |
| Metrics | Deaktiviert | Optional aktiviert |
| Admin-Credentials | Standard | Sicher |
