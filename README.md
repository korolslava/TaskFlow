<div align="center">

# TaskFlow

**Production-grade Project Management Platform**

*Real-time collaboration · Clean Architecture · CQRS · Domain-Driven Design*

[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?style=flat-square\&logo=dotnet)](https://dotnet.microsoft.com)
[![C#](https://img.shields.io/badge/C%23-12.0-239120?style=flat-square\&logo=csharp)](https://learn.microsoft.com/en-us/dotnet/csharp/)
[![EF Core](https://img.shields.io/badge/EF%20Core-8.0-512BD4?style=flat-square)](https://docs.microsoft.com/ef/core/)
[![SignalR](https://img.shields.io/badge/SignalR-WebSocket-FF6B6B?style=flat-square)](https://docs.microsoft.com/aspnet/signalr)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?style=flat-square\&logo=docker)](https://docker.com)
[![xUnit](https://img.shields.io/badge/Tests-xUnit-5C2D91?style=flat-square)](https://xunit.net)
[![License](https://img.shields.io/badge/License-MIT-green?style=flat-square)](LICENSE)

</div>

---

## 📖 Overview

TaskFlow is a full-featured SaaS platform for engineering team management, built to demonstrate production-ready .NET architecture.

It combines:

* **Kanban boards**
* **Sprint planning**
* **Real-time collaboration via SignalR**
* **GitHub integration**

All implemented with industry-standard patterns used in commercial .NET projects.

> Built as a portfolio project to showcase mid-to-senior level .NET engineering:
> Clean Architecture, CQRS, Domain Events, policy-based authorization, background jobs, and integration testing with real infrastructure.

---

## ✨ Features

| Feature                     | Description                                                     |
| --------------------------- | --------------------------------------------------------------- |
| 🗂 **Kanban Boards**        | Drag-and-drop task management with configurable columns         |
| ⚡ **Real-time Updates**     | All board members see changes instantly via SignalR WebSockets  |
| 🏃 **Sprint Planning**      | Backlog management, sprint lifecycle, burn-down charts          |
| 🔐 **Role-based Access**    | Owner / Admin / Member / Viewer per workspace                   |
| 🔗 **GitHub Integration**   | Merging a PR with `#TASK-{id}` automatically moves task to Done |
| 💬 **Comments & @mentions** | Real-time comments with email notifications                     |
| ⏰ **Overdue Alerts**        | Daily Hangfire job notifies assignees                           |
| 🔑 **JWT Auth**             | Access token + refresh token rotation                           |

---

## 🏗️ Architecture

TaskFlow follows **Clean Architecture** with strict separation of concerns.

> Dependencies point inward — Domain has zero external dependencies.

```text
┌──────────────────────────────────────────────┐
│                TaskFlow.API                  │
│ Controllers · Middleware · JWT · Swagger     │
└───────────────────────┬──────────────────────┘
                        │
┌───────────────────────▼──────────────────────┐
│           TaskFlow.Application               │
│ CQRS · Handlers · DTOs · Validation          │
└──────────────┬───────────────┬───────────────┘
               │               │
┌──────────────▼───────┐ ┌─────▼──────────────┐
│ TaskFlow.Domain      │ │ TaskFlow.Infrastructure │
│ Entities · Events    │ │ EF Core · SignalR  │
│ Zero dependencies    │ │ Hangfire · MailKit │
└──────────────────────┘ └────────────────────┘
```

### Key Design Decisions

| Pattern               | Implementation             | Why                       |
| --------------------- | -------------------------- | ------------------------- |
| **CQRS**              | MediatR Commands / Queries | Separation of concerns    |
| **Domain Events**     | MediatR `INotification`    | Decoupled side-effects    |
| **Hybrid ORM**        | EF Core + Dapper           | Performance + flexibility |
| **Auth Policies**     | `IAuthorizationHandler`    | Fine-grained RBAC         |
| **Pipeline Behavior** | ValidationBehavior         | Centralized validation    |

---

## 🛠️ Technology Stack

### Backend

| Layer      | Technology           |
| ---------- | -------------------- |
| Runtime    | .NET 8 · C# 12       |
| Framework  | ASP.NET Core Web API |
| ORM        | EF Core 8            |
| Micro ORM  | Dapper               |
| Real-time  | SignalR              |
| Messaging  | MediatR              |
| Validation | FluentValidation     |
| Auth       | JWT                  |
| Jobs       | Hangfire             |
| Email      | MailKit              |

### Data & Infrastructure

| Component  | Technology      |
| ---------- | --------------- |
| Database   | SQL Server 2022 |
| Containers | Docker          |
| Docs       | Swagger         |

### Testing

| Type        | Tools          |
| ----------- | -------------- |
| Unit        | xUnit · Moq    |
| Integration | TestContainers |

---

## 📂 Project Structure

```text
TaskFlow.sln
├── src/
│   ├── TaskFlow.Domain/               # Zero dependencies
│   │   ├── Entities/                  # Workspace, Project, Board, TaskItem...
│   │   ├── Enums/                     # WorkspaceRole, TaskPriority...
│   │   └── Events/                    # TaskMovedEvent, CommentAddedEvent...
│   │
│   ├── TaskFlow.Application/          # Business logic
│   │   ├── Workspaces/Commands|Queries
│   │   ├── Tasks/Commands|Queries
│   │   ├── Sprints/Commands|Queries
│   │   ├── Comments/Commands|Queries
│   │   ├── Notifications/Handlers/    # SignalR + ActivityLog + Mention handlers
│   │   └── Common/Behaviors/          # ValidationBehavior
│   │
│   ├── TaskFlow.Infrastructure/       # External concerns
│   │   ├── Persistence/               # AppDbContext + Fluent API configs
│   │   ├── RealTime/                  # BoardHub (SignalR)
│   │   ├── BackgroundJobs/            # OverdueTasksJob + MentionEmailJob
│   │   └── Auth/                      # JwtTokenService
│   │
│   └── TaskFlow.API/                  # Entry point
│       ├── Controllers/               # Auth, Workspaces, Boards, Tasks...
│       ├── Authorization/             # WorkspaceRequirement + Handler
│       └── Program.cs
│
└── tests/
    ├── TaskFlow.UnitTests/            # Handlers in isolation (InMemory DB)
    └── TaskFlow.IntegrationTests/     # Full HTTP stack (TestContainers)
```

---

## 📊 Database Schema

```text
Users ──────────────── WorkspaceMembers ──── Workspaces
│                         │                    │
│                      (Role)               Projects
│                                              │
Comments ──── TaskItems ──── Columns ──── Boards
│
Sprints ──── ActivityLogs
```

---

## 🚀 Getting Started

### Prerequisites

* .NET 8 SDK
* Docker

### Run with Docker

```bash
git clone https://github.com/korolslava/TaskFlow.git
cd TaskFlow
docker-compose up --build
```

### Run locally

```bash
docker-compose up db -d

dotnet ef database update \
  --project src/TaskFlow.Infrastructure \
  --startup-project src/TaskFlow.API

dotnet run --project src/TaskFlow.API
```

---

## 🧪 Testing

```bash
dotnet test
```

---

## 🔌 API Endpoints

### Auth

* `POST /auth/register`
* `POST /auth/login`
* `POST /auth/refresh`

### Workspaces

* `POST /workspaces`
* `GET /workspaces/{id}`
* `POST /workspaces/{id}/members`

### Boards & Tasks

* `POST /projects/{id}/boards`
* `GET /projects/{id}/boards/{boardId}`
* `POST /boards/{id}/tasks`
* `PATCH /boards/{id}/tasks/{taskId}/move`

### Sprints

* `POST /projects/{id}/sprints`
* `POST /projects/{id}/sprints/{id}/start`
* `GET /projects/{id}/sprints/{id}/burndown`

---

## ⚡ Real-time (SignalR)

```javascript
connection.invoke("JoinBoard", boardId);

connection.on("TaskMoved", () => {});
connection.on("TaskCreated", () => {});
connection.on("CommentAdded", () => {});
```

---

<div align="center">

Built with ❤️ using ASP.NET Core 8

</div>
