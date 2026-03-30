# Forum Demo

## Overview

Forum Demo is a layered ASP.NET Core Web API that models a small forum-style system with posts, memberships, comments, reactions, and audit events. The application uses in-memory domain event dispatching and SignalR to broadcast post-related activity in real time.

## Features

- User registration and login with JWT authentication
- Public and private posts
- Invitation flow and membership management
- Comments and threaded replies
- Comment reactions
- Post events stored as an audit log
- Real-time notifications using SignalR
- Unit tests with xUnit and Moq

## Architecture

The solution follows a clean layered structure:

- `ForumAPI`: HTTP API, controllers, startup, and middleware
- `ForumBL.Core`: service layer, DTOs, business rules, and abstractions
- `Forum.Domain`: entities, enums, and domain event models
- `Forum.Infrastructure`: repositories, authentication helpers, SignalR hub, and event dispatcher integration
- `Forum.Data`: EF Core `DbContext`, configurations, migrations, and unit of work

Business logic is kept in the service layer rather than controllers. Domain actions publish in-memory events, which are persisted to the `PostEvents` table and broadcast to connected SignalR clients.

## Technologies Used

- .NET 8
- ASP.NET Core Web API
- Entity Framework Core (Code First)
- SQL Server
- SignalR
- xUnit + Moq

## How to Run

1. Start the API:

```bash
dotnet run --project ForumAPI
```

2. Run the tests:

```bash
dotnet test
```

3. Run the SignalR tester:

```bash
dotnet run --project Forum.SignalRTester
```

## Real-Time Testing

The project includes a lightweight console client in `Forum.SignalRTester` for testing SignalR behavior.

- Start the API
- Run one or more tester instances
- Provide a `PostId` and join the related post group
- Trigger actions such as invitations, comments, replies, reactions, or post closing
- Watch `PostUpdated` events appear in real time across connected clients

## Notes / Design Decisions

- An in-memory event dispatcher was used to keep the project simple and focused on application behavior without introducing external infrastructure.
- Layered architecture was chosen to keep domain rules, persistence, transport, and infrastructure concerns separated and easier to extend.
- Authentication is intentionally straightforward with JWT and service-level authorization checks, which keeps the demo readable while still reflecting real backend patterns.
