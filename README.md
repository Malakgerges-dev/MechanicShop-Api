# 🔧 Mechanic Shop Management System

> A production-oriented RESTful API for managing automotive workshop operations, built with **ASP.NET Core 9** and **Clean Architecture**.

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-Web%20API-512BD4)](https://learn.microsoft.com/aspnet/core)
[![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4)](https://learn.microsoft.com/ef/core)
[![SQL Server](https://img.shields.io/badge/SQL%20Server-Database-CC2927?logo=microsoftsqlserver)](https://www.microsoft.com/sql-server)
[![License](https://img.shields.io/badge/License-MIT-green)](LICENSE)

---

## 📌 Overview

**Mechanic Shop Management System** is a backend application designed to digitize and streamline the daily operations of an automotive repair workshop.

The system provides a structured way to manage:

* 👤 Customers
* 🚗 Vehicles
* 🔧 Repair Tasks
* 📋 Work Orders
* 👨‍🔧 Mechanics & Labor
* 📅 Workshop Scheduling
* 🧾 Invoices
* 🧰 Parts
* 💰 Parts & Labor Costs
* 📊 Operational Monitoring

The project was designed with a focus on **maintainability, separation of concerns, domain modeling, performance, security, and production-oriented backend practices**.

---

## 🎯 Business Problem

Automotive workshops often rely on manual or disconnected processes for:

* Tracking customers and their vehicles
* Assigning mechanics
* Managing repair operations
* Scheduling workshop activities
* Tracking work-order status
* Managing parts and labor costs
* Generating invoices
* Monitoring application health and performance

This system provides a centralized backend solution for managing these workflows through a structured RESTful API.

---

## ✨ Key Features

### 🚗 Vehicle Management

* Register and manage customer vehicles
* Associate vehicles with their owners
* Track vehicles involved in workshop operations

### 📋 Work Order Management

* Create work orders for vehicles
* Assign mechanics/labor
* Define workshop spots
* Track work-order lifecycle
* Record start and expected completion times
* Manage repair tasks associated with a work order

### 🔧 Repair Task Management

* Add multiple repair tasks to a work order
* Define estimated repair duration
* Track labor cost
* Associate required parts with each repair task
* Calculate task-level repair costs

### 🧰 Parts Management

* Track parts used during repairs
* Define quantities and unit costs
* Automatically calculate parts costs

### 💰 Invoice Management

* Generate invoices from completed work orders
* Generate invoice line items from repair tasks
* Calculate labor and parts costs
* Apply discounts and taxes
* Track invoice status and payment information
* Generate PDF invoices

### 🔐 Authentication & Authorization

* JWT-based authentication
* ASP.NET Core Identity
* Role and policy-based authorization
* Protected API endpoints
* Secure password management

### 📊 Observability

* Structured logging with Serilog
* Log visualization with Seq
* Distributed tracing with OpenTelemetry
* Prometheus metrics
* Grafana dashboards
* Application health checks
* Request/correlation identifiers

### ⚡ Performance

* In-memory caching
* Hybrid caching
* Output caching
* Rate limiting
* Response compression

---

# 🏗️ Architecture

The application follows **Clean Architecture** principles with a dedicated **Contracts layer** for API-facing request and response models.

```text
┌──────────────────────────────┐
│          API Layer           │
│      ASP.NET Core API        │
│                              │
│ Controllers / Middleware     │
│ Extensions / Composition     │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│       Contracts Layer        │
│                              │
│ Requests / Responses / DTOs  │
│ API-facing contracts         │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│      Application Layer       │
│                              │
│ Use Cases / Features         │
│ Commands / Queries           │
│ Behaviors / Abstractions     │
└──────────────┬───────────────┘
               │
               ▼
┌──────────────────────────────┐
│        Domain Layer          │
│                              │
│ Entities / Business Rules    │
│ Errors / Domain Abstractions │
└──────────────────────────────┘
               ▲
               │
┌──────────────┴───────────────┐
│    Infrastructure Layer      │
│                              │
│ EF Core / SQL Server         │
│ Identity / Caching / PDF     │
│ External Services            │
└──────────────────────────────┘
```

### Architectural Goals

* Separation of concerns
* Dependency inversion
* Maintainability
* Testability
* Isolated business rules
* Replaceable infrastructure implementations
* Clear API boundaries
* Reduced coupling between business logic and infrastructure

### Contracts Layer

The **Contracts layer** contains API-facing models used to communicate between the HTTP layer and the application.

This keeps transport-specific request and response models separated from internal application implementation details.

The result is a clearer API boundary and reduced coupling between the public API contract and internal use-case models.

---

# 📂 Project Structure

```text
src/
│
├── MechanicShop.Api/
│   ├── Controllers/
│   ├── Middlewares/
│   ├── Extensions/
│   └── Program.cs
│
├── MechanicShop.Contracts/
│   ├── Requests/
│   ├── Responses/
│   └── ...
│
├── MechanicShop.Application/
│   ├── Features/
│   ├── Abstractions/
│   ├── Behaviors/
│   └── DependencyInjection/
│
├── MechanicShop.Domain/
│   ├── Entities/
│   ├── Common/
│   ├── Errors/
│   └── Abstractions/
│
└── MechanicShop.Infrastructure/
    ├── Persistence/
    ├── Repositories/
    ├── Identity/
    ├── Caching/
    └── DependencyInjection/
```

---

# 🧠 Domain Model

The domain is centered around the **Work Order**, which represents a vehicle's repair session inside the workshop.

```text
Customer
   │
   └── Vehicle
          │
          └── Work Order
                 │
                 ├── Repair Tasks
                 │      ├── Labor Cost
                 │      └── Parts
                 │
                 ├── Assigned Labor
                 │
                 ├── Workshop Spot
                 │
                 └── Invoice
```

### Work Order

A `WorkOrder` represents a repair session for a vehicle.

It contains information such as:

* Vehicle
* Start time
* Expected/end time
* Assigned labor
* Workshop spot
* Current state
* Repair tasks
* Discount information

### Repair Task

A `RepairTask` represents a specific repair operation performed on a vehicle.

It contains:

* Task name
* Labor cost
* Estimated duration
* Required parts

### Part

A `Part` represents a component consumed during a repair task.

Each part contains:

* Part information
* Quantity
* Unit cost

The repair task cost is calculated from labor and consumed parts:

```text
Task Cost
=
Labor Cost
+
Σ (Part Cost × Quantity)
```

---

# 🧩 Domain-Driven Design

The project applies practical **Domain-Driven Design (DDD)** concepts where they provide real value.

### Aggregate Boundaries

`WorkOrder` represents an important business boundary within the domain.

The model intentionally avoids unnecessarily coupling the entire domain through navigation properties and keeps relationships explicit where appropriate.

This helps:

* Preserve clear aggregate boundaries
* Reduce unnecessary coupling
* Keep domain responsibilities focused
* Prevent the model from becoming unnecessarily complex

> The goal is not to apply DDD patterns everywhere, but to use them where they solve an actual business problem.

---

# 🛡️ Error Handling

The application uses a centralized **Result / Error Pattern** for expected application and business failures.

Instead of treating expected business outcomes as exceptions, operations can return structured results containing meaningful errors.

### Error Categories

```text
Failure
Unexpected
Validation
Conflict
NotFound
Unauthorized
Forbidden
```

Errors also contain structured error codes, for example:

```text
Employee.Id.Required
```

This provides:

* Consistent API error responses
* Predictable error handling
* Better client integration
* Easier debugging
* Clear separation between expected failures and unexpected exceptions

Unexpected exceptions are handled through centralized exception handling at the API boundary.

---

# 🔄 Request Pipeline

A typical request flows through the application as follows:

```text
HTTP Request
     │
     ▼
ASP.NET Core Middleware
     │
     ├── Exception Handling
     ├── Request Logging
     ├── CORS
     ├── Rate Limiting
     └── Authentication / Authorization
     │
     ▼
API Endpoint
     │
     ▼
API Contract
     │
     ▼
Application Layer
     │
     ├── Validation
     ├── Performance
     ├── Caching
     └── Exception Handling
     │
     ▼
Domain Logic
     │
     ▼
Infrastructure Abstractions
     │
     ▼
SQL Server / External Services
     │
     ▼
Application Result
     │
     ▼
HTTP Response
```

---

# 🗄️ Data Access

The application uses:

* **Entity Framework Core 9**
* **SQL Server**
* Repository abstractions
* EF Core migrations
* Configuration-based database connections

Database-related implementation details remain inside the **Infrastructure layer**, keeping the Domain and Application layers independent from the persistence technology.

### Database Approach

The project follows a **Code First** approach.

The general workflow is:

```text
Domain Entities
      │
      ▼
EF Core Configurations
      │
      ▼
DbContext
      │
      ▼
EF Core Migration
      │
      ▼
SQL Server
```

---

# 🔐 Security

Security-related features include:

* JWT Authentication
* ASP.NET Core Identity
* Role-based authorization
* Policy-based authorization
* Secure password handling
* CORS configuration
* Authentication middleware
* Authorization middleware

Sensitive configuration values such as JWT secrets and database credentials should be provided through environment-specific configuration and should never be committed to source control.

---

# ⚡ Performance

The project implements several production-oriented performance techniques.

### Hybrid Caching

HybridCache is used for application-level query caching through a MediatR pipeline behavior.

The architecture uses in-memory caching for fast local access.

### Output Caching

Output caching is used for suitable HTTP endpoints to reduce repeated processing and unnecessary database access.

### Rate Limiting

Rate limiting helps protect API resources from excessive requests and controls resource consumption.

### Response Compression

Response compression reduces HTTP payload sizes and network overhead for supported responses.

---

# 📈 Observability

Observability is treated as part of the application architecture rather than an afterthought.

The project provides visibility into:

* Application behavior
* Request execution
* Performance
* Logs
* Metrics
* Availability

### 📝 Structured Logging

**Serilog** is used for structured application logging.

Logs can contain contextual information such as request identifiers, making troubleshooting easier.

### 🔎 Log Visualization

**Seq** provides a centralized interface for:

* Searching logs
* Filtering events
* Inspecting structured properties
* Investigating application failures

### 📊 Metrics

**Prometheus** collects application metrics by periodically scraping the API metrics endpoint.

### 📉 Dashboards

**Grafana** connects to Prometheus and provides dashboards for monitoring application metrics and system behavior.

### 🔗 Distributed Tracing

**OpenTelemetry** is used to collect tracing information and provide visibility into request execution and application flow.

### ❤️ Health Checks

Health checks provide a standardized way to verify application availability and readiness.

---

# 🧾 Invoice & PDF Generation

The system supports invoice generation based on completed work orders.

The invoice workflow includes:

```text
Completed Work Order
        │
        ▼
Invoice Command
        │
        ▼
Repair Tasks
        │
        ├── Labor
        └── Parts
        │
        ▼
Invoice Line Items
        │
        ▼
Subtotal
        │
        ├── Discount
        └── Tax
        │
        ▼
Final Invoice
        │
        ▼
PDF Generation
```

PDF invoices are generated using **QuestPDF**.

The PDF generation logic is isolated behind an application abstraction so that presentation concerns remain separated from the core business logic.

---

# 📖 API Documentation

The API is documented using **OpenAPI / Swagger**.

Swagger UI provides an interactive interface for:

* Exploring API endpoints
* Viewing request and response models
* Testing API endpoints
* Understanding API contracts
* Reviewing available operations

During local development, Swagger UI is available at:

```text
http://localhost:5194/swagger
```

> **Swagger is currently configured for the Development environment.**

---

# 🐳 Docker & Infrastructure

The project includes containerized infrastructure for local development and integration.

Docker Compose can be used to run supporting services such as:

* API
* SQL Server
* Seq
* Prometheus
* Grafana

The services communicate through a dedicated Docker network.

Example architecture:

```text
                    ┌──────────────────┐
                    │   MechanicShop   │
                    │       API        │
                    └────────┬─────────┘
                             │
          ┌──────────────────┼──────────────────┐
          │                  │                  │
          ▼                  ▼                  ▼
     SQL Server             Seq            Prometheus
                                                │
                                                ▼
                                            Grafana
```

Persistent services use Docker volumes where appropriate so that container restarts do not unnecessarily remove stored service data.

---

# 🚀 Getting Started

## Prerequisites

Make sure you have the following installed:

* [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
* SQL Server
* Git
* Docker Desktop *(optional when running the infrastructure through Docker)*

---

## 📥 Clone the Repository

```bash
git clone https://github.com/Malakgerges-dev/MechanicShopWorkshop.git

cd MechanicShopWorkshop
```

---

## ⚙️ Configuration

Configure the application according to your environment.

Example:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "your-connection-string"
  },

  "Jwt": {
    "Key": "your-secret-key"
  }
}
```

> **Important:** Never commit production secrets, credentials, JWT keys, or private configuration values to source control.

For production environments, use environment variables or an appropriate secrets-management solution.

---

## 🗃️ Database Setup

Apply the existing Entity Framework Core migrations:

```bash
dotnet ef database update
```

Or create a new migration when the model changes:

```bash
dotnet ef migrations add MigrationName
```

Then update the database:

```bash
dotnet ef database update
```

---

## ▶️ Run the Application

### Option 1 — Run with .NET

```bash
dotnet run --project src/MechanicShop.Api
```

Once the application is running, open:

```text
http://localhost:5194/swagger
```

### Option 2 — Run with Docker

Make sure Docker Desktop is running, then execute:

```bash
docker compose up --build
```

The required services will be started according to the Docker Compose configuration.

---

# 🧪 Testing

The project is structured to support automated testing across the application's major layers.

Recommended test organization:

```text
tests/

├── MechanicShop.Domain.Tests/
├── MechanicShop.Application.Tests/
└── MechanicShop.Infrastructure.Tests/
```

The testing strategy focuses on validating:

* Domain business rules
* Application use cases
* Validation behavior
* Error handling
* Infrastructure integrations
* API behavior

---

# 🛠️ Technology Stack

| Technology                | Purpose                               |
| ------------------------- | ------------------------------------- |
| **C#**                    | Programming language                  |
| **.NET 9**                | Application platform                  |
| **ASP.NET Core**          | RESTful Web API                       |
| **API Contracts**         | API request/response models           |
| **Entity Framework Core** | ORM & data access                     |
| **SQL Server**            | Relational database                   |
| **HybridCache**           | Application-level caching             |
| **In-Memory Caching**     | Fast local caching                    |
| **Output Caching**        | HTTP response caching                 |
| **JWT**                   | Authentication                        |
| **ASP.NET Core Identity** | Identity management                   |
| **MediatR**               | Application request/response pipeline |
| **FluentValidation**      | Request validation                    |
| **Serilog**               | Structured logging                    |
| **Seq**                   | Log visualization                     |
| **OpenTelemetry**         | Distributed tracing & observability   |
| **Prometheus**            | Metrics collection                    |
| **Grafana**               | Metrics visualization                 |
| **Health Checks**         | Application health monitoring         |
| **Swagger / OpenAPI**     | API documentation                     |
| **QuestPDF**              | PDF invoice generation                |
| **Docker**                | Containerization                      |
| **Git**                   | Version control                       |

---

# 📐 Engineering Principles

The project follows several software engineering principles:

* Clean Architecture
* SOLID
* Dependency Injection
* Separation of Concerns
* Dependency Inversion
* Domain-driven design principles
* Explicit business rules
* Structured error handling
* API contract separation
* Options Pattern
* Interface-based abstractions
* Production-oriented observability
* Performance-aware API design

---

# 🧭 Future Improvements

Potential future improvements include:

* [ ] Expand automated unit tests
* [ ] Add comprehensive integration tests
* [ ] CI/CD pipeline
* [ ] Cloud deployment
* [ ] Advanced scheduling capabilities
* [ ] Notifications
* [ ] Reporting & analytics
* [ ] Additional operational dashboards
* [ ] Production deployment configuration

---

# 🎓 What This Project Demonstrates

This project goes beyond a basic CRUD application and demonstrates practical backend engineering concepts.

### Architecture

* Designing a layered backend architecture
* Applying Clean Architecture principles
* Managing dependency direction
* Separating API contracts from application logic
* Defining meaningful domain boundaries

### Backend Development

* Building RESTful APIs with ASP.NET Core
* Implementing application use cases with MediatR
* Applying validation and pipeline behaviors
* Working with EF Core and SQL Server
* Designing domain entities and business rules

### Security

* JWT authentication
* ASP.NET Core Identity
* Role and policy-based authorization
* CORS
* Secure configuration management

### Performance

* Hybrid caching
* In-memory caching
* Output caching
* Rate limiting
* Response compression

### Observability

* Structured logging
* Centralized log analysis
* Distributed tracing
* Metrics collection
* Monitoring dashboards
* Health checks
* Request correlation

### Infrastructure

* Docker containerization
* Docker Compose
* SQL Server containers
* Seq
* Prometheus
* Grafana

### Business Logic

* Work-order lifecycle management
* Repair task modeling
* Labor and parts cost calculation
* Discounts and taxes
* Invoice generation
* PDF document generation

---

# 👨‍💻 Author

## Malak Gerges

**.NET Backend Developer**

### Technical Focus

`C#` · `.NET` · `ASP.NET Core` · `REST APIs` · `EF Core` · `SQL Server` · `Clean Architecture` · `SOLID` · `MediatR` · `JWT` · `Identity` · `Serilog` · `OpenTelemetry` · `Docker`

---

## ⭐ Project

If you find the project interesting, feel free to explore the source code and implementation details.

**Built with C#, ASP.NET Core, and a focus on clean, maintainable, and production-oriented backend engineering.**
