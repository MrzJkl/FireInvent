# FireInvent

FireInvent is a modern inventory management system designed for organizations that need to track, assign, and maintain clothing and equipment. Built with .NET 9 and C# 13, it provides a robust API for managing inventory, users, departments, and maintenance workflows.

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

- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [PostgreSQL](https://www.postgresql.org/)
- Docker (optional, for containerized deployment)

### Setup

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