# Tenant Management Guide / Tenant-Verwaltung

## Übersicht / Overview

FireInvent verwendet einen **SystemAdmin-Ansatz** für die Tenant-Verwaltung. Dies ist der Standard-Ansatz für Multi-Tenant SaaS-Anwendungen.

FireInvent uses a **SystemAdmin approach** for tenant management. This is the standard approach for multi-tenant SaaS applications.

## Architektur / Architecture

### 1. SystemAdmin-Rolle / SystemAdmin Role

Eine spezielle Rolle `system-admin` wurde hinzugefügt für Benutzer, die Tenants verwalten sollen.

A special `system-admin` role has been added for users who should manage tenants.

**Keycloak Setup:**
1. Erstelle die Rolle `system-admin` in deinem Keycloak Master-Realm
2. Weise diese Rolle Benutzern zu, die Tenants verwalten sollen
3. Diese Benutzer können dann auf die `/tenants` Endpoints zugreifen

**Keycloak Setup:**
1. Create the `system-admin` role in your Keycloak Master realm
2. Assign this role to users who should manage tenants
3. These users can then access the `/tenants` endpoints

### 2. TenantService mit IgnoreQueryFilters

Der `TenantService` verwendet `IgnoreQueryFilters()` bei allen Datenbank-Operationen, um die normale Tenant-Filterung zu umgehen.

The `TenantService` uses `IgnoreQueryFilters()` for all database operations to bypass normal tenant filtering.

```csharp
// Beispiel: Alle Tenants abrufen (ignoriert Tenant-Filter)
var tenants = await context.Tenants
    .IgnoreQueryFilters()
    .AsNoTracking()
    .OrderBy(t => t.Name)
    .ToListAsync();
```

## API Endpoints

**Basis-URL:** `/tenants`

**Authentifizierung:** Alle Endpoints erfordern die `system-admin` Rolle

### GET /tenants
Listet alle Tenants im System auf.

**Response:**
```json
[
  {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "realm": "fire-dept-berlin",
    "name": "Feuerwehr Berlin",
    "description": "Hauptfeuerwehr",
    "createdAt": "2025-12-08T10:00:00Z"
  }
]
```

### GET /tenants/{id}
Ruft einen spezifischen Tenant ab.

**Response:** Einzelnes Tenant-Objekt oder 404 Not Found

### POST /tenants
Erstellt einen neuen Tenant.

**Request Body:**
```json
{
  "realm": "fire-dept-munich",
  "name": "Feuerwehr München",
  "description": "München Hauptfeuerwehr"
}
```

**Validierung:**
- `realm` muss eindeutig sein (entspricht Keycloak Realm)
- `name` muss eindeutig sein
- Beide Felder sind erforderlich

**Response:** 201 Created mit Tenant-Objekt oder 409 Conflict bei Duplikat

### PUT /tenants/{id}
Aktualisiert einen existierenden Tenant.

**Request Body:** Wie POST

**Response:** 204 No Content oder 404 Not Found oder 409 Conflict

### DELETE /tenants/{id}
Löscht einen Tenant.

**Wichtig:** Schlägt fehl (409 Conflict), wenn der Tenant noch Daten hat:
- Items
- Users
- Oder andere verknüpfte Entitäten

**Response:** 204 No Content oder 404 Not Found oder 409 Conflict

## Workflow: Neuen Tenant erstellen

### Schritt 1: Keycloak Realm erstellen
```bash
# In Keycloak Admin Console
1. Neuen Realm erstellen: "fire-dept-example"
2. Client konfigurieren
3. Benutzer und Rollen einrichten
```

### Schritt 2: Tenant in FireInvent erstellen
```bash
POST /tenants
Authorization: Bearer {token-with-system-admin-role}
Content-Type: application/json

{
  "realm": "fire-dept-example",
  "name": "Feuerwehr Beispiel",
  "description": "Beispiel Feuerwehr für Tests"
}
```

### Schritt 3: Testen
```bash
# Benutzer aus dem neuen Realm authentifizieren sich
# Ihre Requests werden automatisch dem richtigen Tenant zugeordnet
# Sie sehen nur ihre eigenen Daten
```

## Tenant löschen

**Wichtig:** Vor dem Löschen müssen alle Daten entfernt werden!

```bash
# 1. Alle Items, Orders, etc. für diesen Tenant löschen
# 2. Alle Users für diesen Tenant löschen
# 3. Dann Tenant löschen
DELETE /tenants/{id}
```

Oder verwende einen Cascade-Delete-Mechanismus (muss separat implementiert werden).

## Sicherheitsüberlegungen / Security Considerations

### ✅ Vorteile des SystemAdmin-Ansatzes

1. **Einfach zu implementieren** - Keine separate Datenbank erforderlich
2. **Zentralisiert** - Alle Tenant-Operationen an einem Ort
3. **Rollenbasiert** - Verwendet vorhandene Keycloak-Infrastruktur
4. **Audit-Trail** - Alle Änderungen laufen über die API (loggbar)

### 🔒 Sicherheitsmaßnahmen

1. **SystemAdmin-Rolle streng kontrollieren** - Nur vertrauenswürdige Benutzer
2. **IgnoreQueryFilters bewusst verwenden** - Nur im TenantService
3. **Validierung** - Prüft auf Duplikate und Datenintegrität
4. **Audit-Logging** - Empfohlen für alle Tenant-Operationen

## Alternative Ansätze (nicht implementiert)

### 1. Separate Master-Datenbank
**Pro:** Vollständige Trennung von Master- und Tenant-Daten
**Con:** Komplexer, erfordert zwei Datenbankverbindungen

### 2. Super-Tenant
**Pro:** Verwendet vorhandene Tenant-Infrastruktur
**Con:** Kann verwirrend sein, "Tenant verwaltet Tenants"

### 3. Management-Portal
**Pro:** Dedizierte Anwendung für Tenant-Management
**Con:** Zusätzliche Anwendung zu warten

## Häufige Fragen / FAQ

**Q: Kann ein normaler Admin eines Tenants andere Tenants sehen?**
A: Nein. Nur Benutzer mit `system-admin` Rolle können auf `/tenants` zugreifen.

**Q: Was passiert, wenn ich einen Tenant lösche, der noch Daten hat?**
A: Der DELETE-Request gibt 409 Conflict zurück. Daten müssen zuerst gelöscht werden.

**Q: Kann ich die SystemAdmin-Rolle einem Tenant-Benutzer geben?**
A: Ja, aber sie müssen die Rolle im Master-Realm haben, nicht in ihrem Tenant-Realm.

**Q: Wie viele System-Admins kann ich haben?**
A: Beliebig viele - es ist nur eine Keycloak-Rolle.

## Best Practices

1. **Wenige SystemAdmins** - Nur wirklich vertrauenswürdige Personen
2. **Dokumentiere Tenant-Erstellung** - Wer, wann, warum
3. **Teste mit Test-Tenant** - Erstelle einen Test-Tenant für Entwicklung
4. **Backup vor Delete** - Sichere Daten vor dem Löschen eines Tenants
5. **Monitoring** - Überwache `/tenants` Endpoints für verdächtige Aktivität

## Beispiel-Szenario

### Szenario: Neue Feuerwehr onboarden

1. **Sales-Team:** Verkauft FireInvent an "Feuerwehr Köln"
2. **Admin-Team:**
   - Erstellt Keycloak Realm "fire-dept-cologne"
   - Konfiguriert Client und erste Benutzer
   - Sendet Zugangsdaten an Kunde
3. **System-Admin:**
   - Meldet sich mit system-admin Rolle an
   - Erstellt Tenant via `POST /tenants`:
     ```json
     {
       "realm": "fire-dept-cologne",
       "name": "Feuerwehr Köln",
       "description": "Stadt Köln Feuerwehr"
     }
     ```
4. **Kunde:**
   - Meldet sich über subdomain an: `cologne.fireinvent.de`
   - Wird zu Keycloak Realm "fire-dept-cologne" geleitet
   - JWT enthält Realm-Info
   - TenantResolutionMiddleware findet Tenant
   - Kunde sieht nur seine eigenen Daten

Das war's! 🎉
