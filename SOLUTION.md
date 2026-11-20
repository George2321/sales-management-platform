# Design decisions and trade-offs

I started with the requirements and worked from the domain outwards. I wanted to build the parts I enjoy and be able to explain the choices I made in a technical interview. I designed the original domain model myself. I used Codex to challenge my approach, help wire the layers together, and review specific design choices. I reviewed the generated code against the requirements and the conventions I wanted to keep.

## Project structure

I chose a single repository so a reviewer could clone the project and run the Angular UI, ASP.NET Core API, and database together. The UI and API sit side by side under `Presentation/`. The backend is split into `Domain/`, `Application/`, and `Infrastructure/`.

Clean Architecture is familiar to me, and establishing those boundaries early made it easier to decide where each change belonged. The trade-off is a few more projects and interfaces up front. For an assignment of this size, keeping them in one repository makes changes that span the UI and API easy to follow. I would revisit the repository layout if the applications needed independent release cycles or different hosting arrangements.

## Domain model

I used sealed entities for orders, line items, and customers. The entities own the rules that give their operations meaning. For example, a line item requires a positive whole quantity, and an order rejects further status changes once it is fulfilled or cancelled. Keeping those rules in the domain gives every caller the same behaviour.

During review, I moved `OrderTotals` into `Domain/ValueObjects/`. Its subtotal and total are calculated from the order’s line items. It has no identity or lifecycle of its own. I also kept the order status enum under `ValueTypes/` so the distinction is visible in the code.

## Application services

I started with listing orders because it let me verify the full path from the database to the main screen. The Angular component calls its HTTP service, the API controller calls `IOrderService`, and the application service requests the orders through `IOrderRepository`. The repository supplies the results newest first. The list DTO contains the fields the screen needs, including the line-item count, subtotal, and total.

I grouped the order service, its interfaces, and its DTOs under `Application/Features/Orders/`. This keeps the order workflows together and lets the controller use them without knowing how the data is stored.

## Database and persistence

I chose SQLite to keep local setup small and Dapper to keep the SQL visible. I initially gave data access its own project. During review, I moved it under `Infrastructure/Persistence/`, where the SQLite connection factory, schema, seed data, and repository implementation belong. The application owns the repository interface, and Infrastructure implements it. I used Codex to help wire up these dependencies.

I moved storage constraints, such as maximum field lengths and allowed database values, out of the domain entities. SQLite enforces a unique constraint on `ClientProvidedExternalReference`. If the same reference is submitted twice, the second insert leaves the original order intact, and the API returns a conflict with a message the UI can display. Enforcing uniqueness at the point of insertion also covers two submissions arriving at the same time.

## API and error handling

I added a `POST` endpoint to create an order and a `PATCH` endpoint to change its status. As startup grew, I grouped dependency registrations into named extension methods. `Program.cs` still shows when the database is initialized and where the API configures exception handling.

I used one API exception handler to translate domain and application failures into HTTP responses. This keeps error handling consistent across endpoints and avoids repeating `try/catch` blocks in controllers. I also consolidated application failures into one exception type with a failure kind and a specific message.

Reviewing the status endpoint exposed a useful distinction. If someone else has changed the order since it was opened, the caller receives `409 Conflict`. If the caller has the current status and tries to reopen a cancelled order, the domain explains why the change is invalid and the API returns `422 Unprocessable Entity`.

## Angular UI

I removed the Angular starter screen and added routes for the order list and the new-order form. Creating an order has its own page so there is room for multiple line items. The details modal keeps a status edit close to the list.

The first implementation grouped most of the Angular code in one folder. I separated the list, create form, details modal, and supporting utilities under `purchase-orders/`. The typed HTTP service and its request and response models live under `services/purchase-orders/`. This gives each screen a clear home as the feature grows.

## Verification

I checked the list manually to confirm that the newest orders appeared first and that the columns showed the expected values. I also tried creating an order with an existing reference and checked that the UI displayed the duplicate-reference message.

I selected the most important rules from the requirements and used Codex to help set up NUnit coverage. The unit tests exercise quantities, prices, totals, and status transitions directly in the domain. The integration tests send requests through the API against a temporary SQLite database to verify outcomes such as duplicate rejection and newest-first ordering.
