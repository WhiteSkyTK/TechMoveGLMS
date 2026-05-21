# 🚢 TechMove GLMS — Global Logistics Management System

> **EAPD7111 Portfolio of Evidence | ST10296818 — Tokollo Will Nonyane**  
> A full-stack, containerised enterprise logistics platform built with ASP.NET Core, Docker, and JWT authentication.

---

## 📋 Table of Contents

- [Project Overview](#-project-overview)
- [Architecture](#-architecture)
- [Tech Stack](#-tech-stack)
- [Features](#-features)
- [Getting Started — Local](#-getting-started--local-development)
- [Getting Started — Docker](#-getting-started--docker)
- [API Endpoints](#-api-endpoints)
- [Running Tests](#-running-tests)
- [Project Structure](#-project-structure)
- [Screenshots](#-screenshots)

---

## 📖 Project Overview

TechMove Logistics is a global shipping coordinator that previously relied on spreadsheets and manual phone calls. This system — the **Global Logistics Management System (GLMS)** — replaces that legacy workflow with a modern, enterprise-grade web platform.

The system manages:
- **Clients** — company profiles and contact details
- **Contracts** — legal agreements with status tracking (Draft → Active → OnHold → Expired)
- **Service Requests** — freight requests linked to contracts, with live USD→ZAR currency conversion

The platform was built across three parts, evolving from an architecture report → working monolith → fully containerised Service-Oriented Architecture.

---

## 🏗 Architecture

```
┌─────────────────────────────────────────────────────┐
│                   Docker Network                     │
│                                                      │
│  ┌──────────────┐     ┌──────────────────────────┐  │
│  │   sql-server │     │    glms-backend-api       │  │
│  │      -db     │◄────│  ASP.NET Core Web API     │  │
│  │  Port: 1433  │     │  JWT Auth + Swagger        │  │
│  │  SQL Server  │     │  Repository Pattern        │  │
│  │    2022      │     │  Port: 8080               │  │
│  └──────────────┘     └───────────┬──────────────┘  │
│                                   │ HttpClient       │
│                       ┌───────────▼──────────────┐  │
│                       │  glms-frontend-web        │  │
│                       │  ASP.NET Core MVC         │  │
│                       │  Zero DB dependency       │  │
│                       │  Cookie + JWT auth        │  │
│                       │  Port: 8081               │  │
│                       └──────────────────────────┘  │
└─────────────────────────────────────────────────────┘
```

**Separation of concerns:**
- The **API** is the only layer that talks to the database
- The **MVC frontend** only talks to the API via `HttpClient`
- The **Test project** spins up the API in-memory for integration tests

---

## 🛠 Tech Stack

| Layer | Technology |
|---|---|
| Frontend | ASP.NET Core MVC (.NET 9), Razor Views |
| Backend API | ASP.NET Core Web API (.NET 9) |
| Database | Microsoft SQL Server 2022 |
| ORM | Entity Framework Core 9 with Migrations |
| Authentication | JWT Bearer Tokens + Cookie Auth |
| API Docs | Swagger / OpenAPI (Swashbuckle) |
| Currency API | ExchangeRate-API (live USD→ZAR rates) |
| Testing | xUnit, Moq, WebApplicationFactory |
| CI/CD | GitHub Actions |
| Containerisation | Docker + Docker Compose |
| Design Patterns | Repository, Strategy, Factory, Observer |

---

## ✨ Features

### Contract Management
- Create contracts linked to clients with Start/End dates and Service Levels (Standard, Express, Hazardous)
- Upload **PDF signed agreements** — saved to server with UUID naming to prevent overwrites
- Download uploaded agreements from the Contracts list
- Filter contracts by **date range** and **status** using LINQ queries
- Status tracking: `Draft → Active → OnHold → Expired`

### Service Request Processing
- Raise service requests against active contracts
- **Workflow validation** — Expired and On Hold contracts automatically block new requests
- **Live currency conversion** — fetches real-time USD→ZAR rate from ExchangeRate-API
- ZAR equivalent calculated and stored at time of submission

### REST API (Backend)
- Full CRUD endpoints for Clients, Contracts, and Service Requests
- `PATCH /api/contracts/{id}/status` to update status independently
- JWT-protected endpoints — all routes require `Authorization: Bearer {token}`
- Self-documenting via Swagger UI

### Authentication
- JWT tokens issued by the API on login
- MVC stores token in an `HttpOnly` cookie — protected from XSS
- Cookie session for MVC route protection via `[Authorize]`

---

## 🚀 Getting Started — Local Development

### Prerequisites
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9)
- [SQL Server LocalDB](https://learn.microsoft.com/en-us/sql/database-engine/configure-windows/sql-server-express-localdb) (installed with Visual Studio)
- Visual Studio 2022 or VS Code

### Step 1 — Clone and restore
```bash
git clone https://github.com/WhiteSkyTK/TechMoveGLMS.git
cd TechMoveGLMS
dotnet restore
```

### Step 2 — Apply database migrations
```bash
cd TechMoveGLMS.API
dotnet ef database update
```

### Step 3 — Run the API (keep this terminal open)
```bash
cd TechMoveGLMS.API
dotnet run
```
✅ Swagger UI available at: **https://localhost:7100**

### Step 4 — Run the MVC frontend (open a second terminal)
```bash
cd TechMoveGLMS
dotnet run
```
✅ Web app available at: **http://localhost:5273**

### Step 5 — Log in
| Field | Value |
|---|---|
| Username | `admin` |
| Password | `Admin@GLMS2026` |

---

## 🐳 Getting Started — Docker

### Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) installed and **running** (whale icon in taskbar, Linux containers mode)

### Run everything in one command
```bash
# From the solution root (same folder as docker-compose.yml)
docker compose up --build
```

First run downloads SQL Server (~1.5 GB) — allow 5–10 minutes.

| Service | URL | Description |
|---|---|---|
| Swagger API | http://localhost:8080 | Test all API endpoints |
| MVC Web App | http://localhost:8081 | Full frontend application |
| SQL Server | localhost:1433 | Connect via SSMS if needed |

### Stop containers
```bash
docker compose down          # Stop (keeps database data)
docker compose down -v       # Stop and delete all data
```

---

## 📡 API Endpoints

All endpoints require `Authorization: Bearer {token}`. Get a token first from the login endpoint.

### Auth
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/login` | Login — returns JWT token |

### Clients
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/clients` | List all clients |
| GET | `/api/clients/{id}` | Get single client |
| POST | `/api/clients` | Create client |
| PUT | `/api/clients/{id}` | Update client |
| DELETE | `/api/clients/{id}` | Delete client |

### Contracts
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/contracts` | List with optional `?startDate=&endDate=&status=` filter |
| GET | `/api/contracts/{id}` | Get single contract |
| POST | `/api/contracts` | Create contract (multipart/form-data for PDF) |
| PUT | `/api/contracts/{id}` | Full update |
| PATCH | `/api/contracts/{id}/status` | Update status only |
| DELETE | `/api/contracts/{id}` | Delete contract |

### Service Requests
| Method | Endpoint | Description |
|---|---|---|
| GET | `/api/servicerequests` | List all |
| GET | `/api/servicerequests/{id}` | Get single |
| GET | `/api/servicerequests/bycontract/{id}` | Get by contract |
| POST | `/api/servicerequests` | Create with live FX conversion |
| PUT | `/api/servicerequests/{id}` | Update |
| DELETE | `/api/servicerequests/{id}` | Delete |

---

## 🧪 Running Tests

```bash
dotnet test
```

### Test coverage

| Test Class | Count | What is tested |
|---|---|---|
| `CurrencyCalculationTests` | 12 | USD→ZAR math, edge cases (zero, negative, rounding) |
| `FileValidationTests` | 11 | PDF validation, forbidden extensions, null/empty inputs |
| `DocumentServiceTests` | 8 | File upload service (exe, docx, jpg, zip rejection) |
| `WorkflowValidationTests` | 14 | Contract eligibility, status rules, date validation |
| `ApiIntegrationTests` | 12 | Real HTTP calls against in-memory API (login, CRUD, 401s) |
| **Total** | **57** | |

Integration tests use `WebApplicationFactory<Program>` with an EF Core in-memory database — no SQL Server required to run tests.

### CI/CD
Every push to `main` triggers the GitHub Actions pipeline (`.github/workflows/dotnet-ci.yml`) which builds the solution and runs all tests automatically.

[![CI Pipeline](https://github.com/WhiteSkyTK/TechMoveGLMS/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/WhiteSkyTK/TechMoveGLMS/actions)

---

## 📁 Project Structure

```
TechMoveGLMS.sln
│
├── TechMoveGLMS/                    ← MVC Frontend (presentation only)
│   ├── Controllers/                 ← Calls IApiClientService, no DbContext
│   │   ├── HomeController.cs
│   │   ├── ClientsController.cs
│   │   ├── ContractsController.cs
│   │   ├── ServiceRequestsController.cs
│   │   └── AuthController.cs        ← Login/logout, stores JWT cookie
│   ├── Services/
│   │   ├── IApiClientService.cs     ← Interface for all API calls
│   │   └── ApiClientService.cs      ← HttpClient implementation
│   ├── Views/                       ← Razor views (site.css design system)
│   ├── Program.cs                   ← HttpClient + Cookie auth, NO DbContext
│   └── Dockerfile
│
├── TechMoveGLMS.API/                ← Web API Backend (owns all DB access)
│   ├── Controllers/
│   │   ├── AuthController.cs        ← POST /api/auth/login → JWT
│   │   ├── ClientsController.cs
│   │   ├── ContractsController.cs   ← Includes PATCH /status
│   │   └── ServiceRequestsController.cs
│   ├── Repositories/
│   │   ├── IRepositories.cs         ← IClientRepository, IContractRepository, etc.
│   │   └── Repositories.cs          ← EF Core implementations
│   ├── DTOs/
│   │   └── Dtos.cs                  ← LoginDto, ContractStatusUpdateDto, etc.
│   ├── Program.cs                   ← JWT + Swagger + DI + CORS + auto-migrate
│   ├── appsettings.json
│   └── Dockerfile
│
├── Tests/                           ← Unit + Integration tests (xUnit)
│   ├── Integration/
│   │   └── ApiIntegrationTests.cs   ← WebApplicationFactory tests
│   ├── CurrencyCalculationTests.cs
│   ├── DocumentServiceTests.cs
│   ├── FileValidationTests.cs
│   ├── WorkflowValidationTests.cs
│   └── Tests.csproj
│
├── docker-compose.yml               ← 3-container orchestration
└── .github/
    └── workflows/
        └── dotnet-ci.yml            ← GitHub Actions CI pipeline
```

---

## 📸 Screenshots

> <img width="1360" height="768" alt="Screenshot 2026-05-17 185102" src="https://github.com/user-attachments/assets/69683420-c833-446a-ac9d-5b945b75ba89" />
<img width="1360" height="768" alt="Screenshot 2026-05-17 185053" src="https://github.com/user-attachments/assets/e096d71f-eb87-4af4-9b92-808e03c78e5f" />
<img width="1360" height="768" alt="Screenshot 2026-05-17 185033" src="https://github.com/user-attachments/assets/832fedbe-7c07-4472-870e-2fa212b8df4e" />
<img width="1360" height="768" alt="Screenshot 2026-05-17 185022" src="https://github.com/user-attachments/assets/31ce38fa-c11c-4a24-88a5-273b8f6c3250" />
<img width="1360" height="768" alt="Screenshot 2026-05-17 192549" src="https://github.com/user-attachments/assets/9e929424-a370-4485-831b-dd1f9713d51b" />
<img width="1360" height="768" alt="Screenshot 2026-05-17 190351" src="https://github.com/user-attachments/assets/7757d7f9-2ffb-4f8f-9fd1-5443dc7e8623" />
<img width="1360" height="768" alt="Screenshot 2026-05-17 185126" src="https://github.com/user-attachments/assets/ba0a0d89-4ce4-4028-9410-ce205ee74492" />


| | |
|---|---|
| Dashboard | Contracts list with filter |
| Raise service request | Swagger UI |
| Docker containers running | Tests passing |

---

## 👤 Student Information

| Field | Detail |
|---|---|
| Student Number | ST10296818 |
| Name | Tokollo Will Nonyane |
| Module | EAPD7111 |
| Institution | The Independent Institute of Education |
| Year | 2026 |

---

## 📄 License

This project was built as an academic submission. All rights reserved © 2026 Tokollo Will Nonyane.
