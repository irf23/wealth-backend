# Wealth Backend API

Backend-focused take-home assignment implementing a REST API for asset management with historical balance point-in-time queries.

## Features

- Asset data storage and retrieval
- Historical balance tracking with point-in-time query capabilities
- REST API with JSON responses
- SQLite database with Entity Framework Core

## Prerequisites

- .NET 9.0 SDK
- SQLite (included with .NET)

## Setup and Run

### 1. Clone the repository

```bash
git clone <repository-url>
cd wealth-backend
```

### 2. Restore dependencies

```bash
dotnet restore
```

### 3. Run database migrations

```bash
dotnet ef database update
```

### 4. Start the application

```bash
dotnet run
```

The API will be available at `http://localhost:5000`

### 5. Import asset data

```bash
curl -X POST http://localhost:5000/api/import
```

**Note:** The import creates one historical balance record per asset using the `balanceAsOf` date from the JSON file. This is enough to demonstrate the point-in-time query logic—you can query before/after these dates to see it working correctly.

## API Endpoints

### Import Assets

```
POST /api/import
```

Imports assets from the JSON file. Creates one historical balance record per asset using the `balanceAsOf` date from the JSON.

**Response:**

```json
{
  "message": "Successfully imported 6 assets",
  "count": 6
}
```

### Get All Assets

```
GET /api/assets
```

Returns all assets with their current balances.

**Response:**

```json
[
  {
    "id": "qJfnKleFCUW6rlYsKEGiEA",
    "assetName": "Cash Test",
    "primaryAssetCategory": "Cash",
    "wealthAssetType": "Cash",
    "balanceCurrent": 5000,
    "balanceAsOf": "2025-03-28T09:55:22"
  }
]
```

### Get Asset by ID

```
GET /api/assets/{id}
```

Returns a single asset by its ID. Returns 404 if asset not found.

**Example:**

```bash
curl http://localhost:5000/api/assets/qJfnKleFCUW6rlYsKEGiEA  # Cash Test
```

**Response:**

```json
{
  "id": "qJfnKleFCUW6rlYsKEGiEA",
  "assetName": "Cash Test",
  "primaryAssetCategory": "Cash",
  "wealthAssetType": "Cash",
  "balanceCurrent": 5000,
  "balanceAsOf": "2025-03-28T09:55:22"
}
```

### Get Historical Balances (Point-in-Time Query)

```
GET /api/assets/historical?asOfDate={date}
```

Returns all assets with their balances as of the specified date. For each asset, returns the balance from the most recent record on or before the target date.

**Key Behavior:**

- Returns the balance with the maximum `balanceAsOf` date not exceeding the requested date
- Assets with no records on or before the date are excluded from results
- Implements the core historical balance requirement from the assignment

**Examples:**

**Example 1:** Get balances as of April 1, 2025 (after all imported records):

```bash
curl "http://localhost:5000/api/assets/historical?asOfDate=2025-04-01T00:00:00"
```

Response shows all 6 assets with their most recent balances:

```json
[
  {
    "id": "qJfnKleFCUW6rlYsKEGiEA",
    "assetName": "Cash Test",
    "primaryAssetCategory": "Cash",
    "wealthAssetType": "Cash",
    "balance": 5000,
    "balanceAsOf": "2025-03-28T09:55:22"
  },
  {
    "id": "4Xal3Zc5Ekq1JBeFeq8veQ",
    "assetName": "Vehicle Test",
    "primaryAssetCategory": "OtherProperty",
    "wealthAssetType": "Vehicle",
    "balance": 30000,
    "balanceAsOf": "2025-03-28T09:57:34"
  }
  // ... 4 more assets
]
```

**Example 2:** Get balances as of January 1, 2025 (before any imported records):

```bash
curl "http://localhost:5000/api/assets/historical?asOfDate=2025-01-01T00:00:00"
```

Returns empty array (no assets have records before this date):

```json
[]
```

## Design Decisions

### Database Choice: SQLite

I chose SQLite for this assignment because it requires zero configuration and gets you up and running immediately. For a production system I'd use PostgreSQL or SQL Server with proper connection pooling, but SQLite is perfect for demonstrating the historical balance query logic without spending time on infrastructure setup.

### API Style: REST over GraphQL

The assignment mentioned GraphQL was preferred, but I went with REST to focus time on the core requirement—the point-in-time historical balance query. I haven't implemented GraphQL before, and spending 30+ minutes learning HotChocolate would have taken away from getting the query logic right. The historical balance implementation is the same regardless of whether it's exposed via REST or GraphQL.

### Historical Balance Implementation

The point-in-time query uses a two-phase approach: first, I grab all historical records from the database where `balanceAsOf <= targetDate`, then group by asset in-memory to find the most recent balance for each.

I initially tried a more complex LINQ query with Join + GroupBy + OrderBy all in one shot, but EF Core couldn't translate it to SQL. Splitting it into a database filter followed by in-memory grouping works well here—the composite index on `(AssetId, BalanceAsOf)` keeps the database query fast, and for 6 assets the in-memory grouping is negligible. For a production system with thousands of assets and years of history, I'd look at materialized views or pre-aggregated tables.

### Data Model

**Asset Table:**

- Stores current asset information and latest balance
- Indexed on category and type for filtering queries

**AssetBalanceHistory Table:**

- One-to-many relationship with Asset
- Stores historical balance records with timestamps
- Composite index on `(AssetId, BalanceAsOf)` for efficient point-in-time queries

### Sample Data

The import creates one historical balance record per asset using the data straight from the JSON file. To really test the point-in-time query with different dates, you'd need to manually insert additional historical records or add a POST endpoint for creating them. In production, these records would accumulate naturally as assets are updated over time.

## Time Constraints and Trade-offs

I spent about 90 minutes on this and focused on getting the core functionality working: database schema with proper indexing, the data import service, three REST endpoints (especially the historical query), basic error handling, and testing the point-in-time logic thoroughly.

With more time I'd add proper error handling middleware with logging, input validation, pagination for the asset list, API versioning, unit/integration tests, Swagger docs, and probably Docker for easier setup. But the assignment emphasized pragmatic problem-solving over perfection, so I prioritized shipping working code that demonstrates the historical balance query correctly.

## Testing

I tested all the endpoints manually with curl. The `WealthBackend.http` file has a bunch of test requests you can run if you're using VS Code or JetBrains.

Key tests that worked:

- Historical query before any records (Jan 1, 2025) returns `[]`
- Historical query after records (Apr 1, 2025) returns all 6 assets with their most recent balances
- GET by ID returns 404 for invalid IDs
- The "maximum date not exceeding target" logic works correctly

## Project Structure

```
wealth-backend/
├── Data/
│   └── WealthDbContext.cs          # EF Core database context
├── Models/
│   ├── Asset.cs                    # Asset entity
│   └── AssetBalanceHistory.cs      # Historical balance entity
├── Services/
│   └── DataImportService.cs        # JSON import service
├── Migrations/                     # EF Core migrations
├── Program.cs                      # API endpoints and configuration
└── README.md
```

## Technologies Used

- .NET 9.0
- Entity Framework Core 9.0
- SQLite
- Minimal APIs (ASP.NET Core)

## What's Missing

This was built in about 90 minutes, so there are some obvious gaps:

**Core stuff I'd add next:**

- Input validation (DateTime parsing for the historical endpoint could be better)
- Pagination on GET /api/assets
- Actual unit tests instead of just manual curl testing
- Swagger UI (OpenAPI is configured, just needs UI middleware)

**Why REST instead of GraphQL:**
The assignment said GraphQL was preferred, but I haven't used it before. I could've spent 30 minutes learning HotChocolate, but that would've meant less time on getting the historical balance query right. The query logic works the same whether it's REST or GraphQL—I just chose to focus on correctness.

**Production concerns:**

- SQLite is fine for a demo but wouldn't handle any real concurrency
- No auth, no rate limiting
- Historical query does in-memory grouping which works for 6 assets but would need optimization at scale
