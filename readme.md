# FireInvent

FireInvent is a modern inventory management system designed for organizations that need to track, assign, and maintain clothing and equipment. Built with .NET 10 and C# 13, it provides a robust API for managing inventory, users, departments, and maintenance workflows.

## Features

- **Inventory Management**: Track clothing products, variants, and individual items.
- **Assignment Tracking**: Manage assignment history of items to persons and departments.
- **Maintenance Logging**: Record and monitor maintenance activities for each item.
- **User Management**: Synchronize users via OpenID Connect and manage user data.
- **API Integration Management**: Create and manage third-party API access credentials for integrations.
- **Health Checks**: Built-in endpoints for system and database health monitoring.
- **Secure Authentication**: JWT Bearer authentication and OpenID Connect integration.
- **Swagger/OpenAPI**: Interactive API documentation and testing via Swagger UI.
- **Logging**: Structured logging with Serilog for diagnostics and auditing.

## Project Structure

- **Api/**  
  Contains the ASP.NET Core Web API, including configuration, middleware, authentication, and controllers.

- **Database/**  
  Entity Framework Core models and migrations for PostgreSQL database integration.

- **Shared/**  
  Shared services, models, and business logic used across the API and other components.

- **Contract/**  
  Common contracts and constants for data validation and cross-project consistency.

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (for local development)
- [PostgreSQL](https://www.postgresql.org/) (for local development)
- [Docker](https://www.docker.com/) and [Docker Compose](https://docs.docker.com/compose/) (for deployment)

### Local Development Setup

1. **Clone the repository**

2. **Configure the database**
- Set up a PostgreSQL instance.
- Update the connection string in `Api/appsettings.json` under `ConnectionStrings:DefaultConnection`.

3. **Configure authentication**
- Set OpenID Connect settings in `Api/appsettings.json` under `Authentication`.
- Configure Keycloak Admin API settings under `KeycloakAdmin` for API integration management.

4. **Run database migrations**

5. **Start the API**


6. **Access Swagger UI**
- Open [http://localhost:5000/swagger](http://localhost:5000/swagger) in your browser.

## Deployment

FireInvent uses Docker Compose for production deployment with the following services:
- **Caddy**: Reverse proxy with automatic HTTPS
- **Keycloak**: Identity and access management
- **API**: FireInvent backend API
- **UI**: FireInvent frontend application
- **PostgreSQL**: Databases for Keycloak and API

### Deployment Prerequisites

- Docker and Docker Compose installed on your server
- Domain names configured and pointing to your server (or use local development domains)
- SMTP server for email notifications (optional, but recommended)

### Deployment Steps

1. **Navigate to the deployment directory**
   ```bash
   cd deployment
   ```

2. **Create and configure the environment file**
   ```bash
   cp .env.example .env
   ```
   
   Edit the `.env` file and configure the following:
   
   **Required Configuration:**
   - `API_DB_PASSWORD`: Secure password for the API database
   - `KEYCLOAK_DB_PASSWORD`: Secure password for the Keycloak database
   - `KEYCLOAK_ADMIN_PASSWORD`: Secure password for Keycloak admin user
   - `API_DOMAIN`: Your API domain (e.g., `api.example.com`)
   - `AUTH_DOMAIN`: Your authentication domain (e.g., `auth.example.com`)
   - `UI_DOMAIN`: Your UI domain (e.g., `example.com`)
   - `KEYCLOAK_HOSTNAME`: Should match `AUTH_DOMAIN`
   
   **Optional Configuration:**
   - SMTP settings for email notifications
   - Custom database names and users
   - CORS origins (if needed)

   **For production HTTPS:**
   - Update `FIREINVENT_AUTHORITY` to `https://${AUTH_DOMAIN}/realms/fireinvent`
   - Update `FIREINVENT_SPA_URL` to `https://${UI_DOMAIN}`
   - Ensure `VITE_API_BASE_URL` and `VITE_KEYCLOAK_URL` use your production domains (without `https://` prefix)

3. **Create the proxy network**
   ```bash
   docker network create proxy_net
   ```

4. **Start the services**
   ```bash
   docker compose up -d
   ```

   This will:
   - Pull all required Docker images
   - Start PostgreSQL databases for API and Keycloak
   - Start Keycloak with automatic realm configuration
   - Start the FireInvent API
   - Start the FireInvent UI
   - Start Caddy reverse proxy with automatic HTTPS

5. **Verify deployment**
   
   Check that all services are running:
   ```bash
   docker compose ps
   ```
   
   All services should show as "healthy" or "running".

6. **Access the application**
   
   - UI: `https://${UI_DOMAIN}` (or `http://localhost` for local testing)
   - API: `https://${API_DOMAIN}` (or `http://api.localhost` for local testing)
   - Keycloak: `https://${AUTH_DOMAIN}` (or `http://auth.localhost` for local testing)

7. **First login**
   
   - Navigate to the UI
   - Click on login
   - Use the Keycloak admin credentials from your `.env` file
   - After first login, create additional users as needed

### Development Deployment

For local development with a simpler setup, use the development compose file for Keycloak:

1. Edit `deployment/compose.yml` and change:
   ```yaml
   - ./keycloak/compose.prod.yml
   ```
   to:
   ```yaml
   - ./keycloak/compose.dev.yml
   ```

2. In your `.env` file, use local development URLs:
   ```bash
   FIREINVENT_AUTHORITY=http://localhost:8080/realms/fireinvent
   FIREINVENT_SPA_URL=http://localhost:3000
   KEYCLOAK_HOSTNAME=localhost
   ```

3. The development setup exposes Keycloak directly on port 8080 for easier debugging.

### Updating Services

To update to the latest version:

```bash
cd deployment
docker compose pull
docker compose up -d
```

This will pull the latest images and recreate containers if needed while preserving data.

### Viewing Logs

To view logs for all services:
```bash
docker compose logs -f
```

To view logs for a specific service:
```bash
docker compose logs -f api
docker compose logs -f keycloak
docker compose logs -f ui
```

### Stopping Services

To stop all services:
```bash
docker compose down
```

To stop and remove all data (databases, certificates, etc.):
```bash
docker compose down -v
```
**Warning:** This will delete all data including databases!

### Backup

To backup your data, backup the Docker volumes:

```bash
# Backup API database
docker compose exec db_api pg_dump -U ${API_DB_USER} ${API_DB_NAME} > api_backup.sql

# Backup Keycloak database
docker compose exec db_keycloak pg_dump -U ${KEYCLOAK_DB_USER} ${KEYCLOAK_DB_NAME} > keycloak_backup.sql
```

### Troubleshooting

**Services not starting:**
- Check logs with `docker compose logs`
- Ensure all required environment variables are set in `.env`
- Verify Docker network exists: `docker network ls | grep proxy_net`

**Cannot access services:**
- For local testing, ensure your `/etc/hosts` file contains:
  ```
  127.0.0.1 localhost api.localhost auth.localhost
  ```
- For production, verify DNS records point to your server
- Check firewall rules allow ports 80 and 443

**Keycloak configuration not applying:**
- Check `keycloak_config` container logs
- Verify configuration files exist in `deployment/keycloak/config/`
- Ensure environment variables are correctly substituted

## API Overview

- **Authentication**: JWT Bearer and OpenID Connect.
- **Endpoints**: CRUD operations for users, departments, clothing products, variants, items, assignments, and maintenance.
- **API Integrations** (Admin-only): Create, list, and delete API access credentials for third-party integrations via confidential clients in Keycloak.
- **Health**: `/health` endpoint for system status.
- **Swagger**: `/swagger` for API documentation and testing.

### API Integration Management

Administrators can create API integrations to allow third-party applications to access the FireInvent API:

1. **Create Integration**: POST to `/api-integrations` with a name and optional description.
   - Returns client ID and secret (shown only once)
   - Creates a confidential client in Keycloak with service account enabled
   
2. **List Integrations**: GET `/api-integrations` to view all existing integrations.

3. **Delete Integration**: DELETE `/api-integrations/{clientId}` to revoke access.

Integrations use the OAuth 2.0 client credentials grant flow to obtain access tokens.

## Contributing

Contributions are welcome! Please open issues or submit pull requests for improvements or bug fixes.

## License

This project is licensed under the GPL-2.0 License.