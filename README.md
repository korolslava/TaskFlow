<div align="center">

# TaskFlow

![.NET](https://img.shields.io/badge/.NET-8.0-blueviolet)
![C#](https://img.shields.io/badge/C%23-12.0-239120)
![EF Core](https://img.shields.io/badge/ORM-EF%20Core-blue)
![Dapper](https://img.shields.io/badge/Micro--ORM-Dapper-green)
![SignalR](https://img.shields.io/badge/Realtime-SignalR-orange)
![SQL Server](https://img.shields.io/badge/DB-SQL%20Server-red)
![xUnit](https://img.shields.io/badge/Tests-xUnit-brightgreen)
![Railway](https://img.shields.io/badge/Deploy-Railway-black)

**Project management API** — Kanban boards, sprint planning, real-time collaboration via SignalR.
Deployed on Railway with Azure SQL Database.

[**Live Swagger →**](https://taskflow-1.up.railway.app/index.html)

</div>

---

## Stack

**ASP.NET Core 8** · **EF Core 8** (writes) + **Dapper** (reads) · **SignalR** · **MediatR** · **Hangfire** · **SQL Server on Azure**

---

## Architecture

Clean Architecture with 4 layers. Domain has zero external dependencies.

```
TaskFlow.API          → Controllers, JWT auth, SignalR hub mapping, Swagger
TaskFlow.Application  → CQRS handlers (MediatR), FluentValidation pipeline, DTOs
TaskFlow.Infrastructure → AppDbContext (EF Core + Fluent API), Dapper queries,
                          BoardHub (SignalR), Hangfire jobs, MailKit
TaskFlow.Domain       → Entities, Enums, Domain Events (INotification) — no deps
```

**Key decisions:**

* CQRS via MediatR — separate Command/Query handlers, ValidationBehavior in pipeline
* Domain Events — `TaskMovedEvent` is published after `MoveTaskCommand`, handled by three handlers in parallel: SignalR broadcast, ActivityLog write, @mention parsing
* EF Core for writes with Fluent API configuration, Dapper for analytical queries (GetBoard JOIN, BurnDown aggregation)
* Policy-based authorization — `WorkspaceAuthHandler` checks user role within a specific workspace on each request
* JWT + refresh token rotation — refresh token stored in DB, revoked via `/auth/refresh`

---

## Features

|                    |                                                                                                             |
| ------------------ | ----------------------------------------------------------------------------------------------------------- |
| **Kanban**         | Columns + tasks with Order field for drag-and-drop, `MoveTaskCommand` updates ColumnId + Order              |
| **Real-time**      | `BoardHub` groups connections by `board:{id}`, broadcasts events after MoveTask/CreateTask/Comment          |
| **Sprints**        | Lifecycle: Planning → Active → Completed, burn-down chart via Dapper (points per day)                       |
| **RBAC**           | 4 roles (Owner/Admin/Member/Viewer) per workspace, enforced via `IAuthorizationRequirement`                 |
| **GitHub webhook** | PR merged with `#TASK-{guid}` → automatically moves task to Done column, HMAC-SHA256 signature verification |
| **@mentions**      | Regex parsing in comments → Hangfire fire-and-forget → email via MailKit                                    |
| **Overdue alerts** | Hangfire recurring job daily at 08:00, Dapper query for overdue tasks                                       |
| **Activity log**   | Every Domain Event is written to `ActivityLogs` table via `INotificationHandler`                            |

---

## Database

SQL Server on **Azure SQL Database**. Schema via EF Core Fluent API:

* Composite PK on `WorkspaceMembers (WorkspaceId, UserId)`
* Index on `(ColumnId, Order)` for fast task ordering
* Index on `(WorkspaceId, OccurredAt)` for activity log queries
* `DeleteBehavior.Cascade` across hierarchy Workspace → Projects → Boards → Columns → Tasks
* Enums stored as `int` via `.HasConversion<int>()`

---

## Tests

```bash
dotnet test tests/TaskFlow.UnitTests
```

9 unit tests — `CreateWorkspaceHandler`, `MoveTaskHandler`, `CreateTaskHandler` using xUnit + Moq + InMemory DB.
Validated:

* DB persistence
* Domain event publishing (`mediatorMock.Verify`)
* Guard clauses (task not found → exception)

---

## Run locally

```bash
# SQL Server in Docker
docker-compose up db -d

# Migrations
dotnet ef database update \
  --project src/TaskFlow.Infrastructure \
  --startup-project src/TaskFlow.API

# API
dotnet run --project src/TaskFlow.API
# Swagger: https://localhost:{port}/swagger
```

**appsettings.Development.json** — requires connection string and Jwt section (see `appsettings.json` as template).

---

## API

```
POST   /auth/register                              — register
POST   /auth/login                                 — JWT + refresh token
POST   /auth/refresh                               — refresh token rotation

POST   /workspaces                                 — create workspace [Authorize]
POST   /workspaces/{id}/members                    — invite member [Admin+]

POST   /projects/{workspaceId}/boards              — create board [Admin+]
GET    /projects/{workspaceId}/boards/{boardId}    — board with columns and tasks [Member+]

POST   /boards/{boardId}/tasks                     — create task [Member+]
PATCH  /boards/{boardId}/tasks/{taskId}/move       — move task [Member+]

POST   /projects/{projectId}/sprints               — create sprint [Admin+]
POST   /projects/{projectId}/sprints/{id}/start    — start sprint [Admin+]
GET    /projects/{projectId}/sprints/{id}/burndown — burn-down data [Member+]

POST   /tasks/{taskId}/comments                    — add comment [Member+]

POST   /webhooks/github                            — GitHub PR webhook
```

---

## SignalR

```
wss://.../hubs/board?access_token={jwt}
```

→ `JoinBoard(boardId)`

Events:

* `TaskMoved`
* `TaskCreated`
* `CommentAdded`
