

## Initial Git Setup
```
Setup my git folder, I will be building a dotnet + angular minimal application.
Ensure relevant folders are in the .gitignore for this stack.
```
---
## Scaffolding the Application
```markdown
### Task
Scaffold a complete full-stack .NET and Angular application. Use .NET Aspire to orchestrate the local development environment and generate a Docker Compose file for potential production deployment.

### Tech Stack
- **Backend:** .NET 10 Web API
- **Database ORM:** Entity Framework Core (EF Core) using SQLite (file-system database)
- **Frontend:** Angular v22 (Standalone components)
- **Local Orchestration:** .NET Aspire
- **Deployment Orchestration:** Docker Compose
- **Configuration Management:** .env files

### Requirements
1. **Application Structure:** Create a clean directory structure separating the `Backend` (API), `Frontend` (Angular SPA), and `Aspire` (AppHost/ServiceDefaults).
2. **Configuration Setup:** Ensure both the .NET API and the Angular frontend read their configurable settings (like API URLs and the SQLite database file path) from a `.env` file. Do not hardcode these in `appsettings.json` or `environment.ts`.
3. **Database Setup:** Scaffold a basic EF Core DbContext using SQLite. Map the SQLite connection string (e.g., `Data Source=app.db`) to the `.env` file configuration.
4. **Aspire Orchestration:** Configure the Aspire AppHost to natively spin up the Backend API and the Angular frontend server.
5. **Docker Compose:** Generate a `docker-compose.yml` file at the root to run the API and the Frontend (served via an Nginx container). Ensure a volume is mapped for the SQLite `.db` file so data persists between container restarts.

### Verification Steps
Before declaring this task complete, you must verify your work:
1. Run `dotnet build` on the entire solution to ensure there are no compilation errors.
2. Verify that the frontend compiles successfully (e.g., `npm run build` or `ng build`).
3. Ensure the `.env` loading logic is properly implemented in the backend `Program.cs`.
4. Validate that the `docker-compose.yml` syntax is correct, maps the `.env` variables, and includes the volume mount for SQLite.
Read the outputs of these checks and fix any errors before finishing.
```

---

## Styling the Frontend
```markdown
Lets install tailwind on the Angular side and make use of ShadCN as the component library.

I want basic stuff like inputs, buttons, labels, modals, tables, navbars, cards.

Create a basic layout with a Navbar and content section. The Navbar should have a section for Customers and a Section For Orders.
```

--- 

## Domain Models
```markdown
Create on the API side the following models.

 public enum OrderStatus
{
    Pending,
    Confirmed,
    Fulfilled,
    Cancelled
}

public class Customer
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
}

public class LineItem
{
    public string Sku { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal => Quantity * UnitPrice;
}

public class Order
{
    public Guid Id { get; set; }
    public string ClientReference { get; set; } = string.Empty;
    public Customer Customer { get; set; } = default!;
    public List<LineItem> Items { get; set; } = new();
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public string Currency { get; set; } = "USD";
    public string? Notes { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public decimal Subtotal => Items.Sum(item => item.LineTotal);
    public decimal Total => Subtotal;
}


    Make sure to set up restrictions, a order cannot exist without a customer, apply database unique restriction on customerId + order-reference number.

    We mix domain model and data models for now and should mention as an explicit trade off in an ADR.
```