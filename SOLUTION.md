# Design decisions and trade-offs

First, I checked with AI whether the approach I had in mind was sound according to best practices and current industry trends. I enjoy coding and wanted this assignment to show my own thinking. I expect a technical interview and want to be able to explain my approach and the decisions I made.

## Project design

I chose Clean Architecture and built the layers from the domain out. I kept the UI and API in separate folders within the same repository to make the project easier to start and run, as the assignment requires.

These layers are familiar to me from other projects, so I know where to place code as the application grows. I prefer to establish those boundaries early because changing the architecture later can be difficult.

Setting up the layers takes a little extra work at the start, but it keeps the project consistent over time. A monorepo keeps an assignment of this size simple. For a larger application, I would also consider the expected throughput and how the application will be hosted.

## Domain design

I used object-oriented principles to design the domain classes. The entities are sealed to make it clear that they do not support inheritance. Organising value types and value objects into their own folders makes their purpose easier to recognise.

I kept business rules on the domain entities so that every caller follows the same rules. The domain is the innermost layer and has no dependencies on the other layers.

## Database

I chose Dapper and SQLite because they are straightforward to set up for this project. I initially kept data access in its own layer to keep the database code together.

The `Orders` table has a unique constraint on `ClientProvidedExternalReference` to prevent duplicate orders. Check constraints enforce field lengths and allowed values. Enforcing these rules in the database protects data integrity even when a request bypasses front-end validation.

## Application layer

I created the order service, DTOs, and interfaces in the Application layer. The service coordinates the order operations, and the repository handles data access. This keeps both responsibilities out of the UI.

## API

As the startup file grew, I grouped the dependency injection registrations into named extension methods so the startup sequence was easier to follow.

I added exception-handling middleware in the API to turn domain and repository errors into consistent, user-friendly responses. This avoids repeating `try/catch` blocks in each controller or application service.

## Connecting Angular and the API

I started with the order list so I could follow a request from the main screen through to the database.

1. I added an asynchronous `GET` endpoint and a repository method to retrieve the orders.
2. I created the Angular list component, service, and route for the sales management screen and removed the default starter content.
3. I added a `POST` endpoint to create an order and return a helpful message if its reference already exists.
4. I added a `PATCH` endpoint to update an order’s status.

I reorganised the Angular features and services into separate folders. Keeping them all in one folder was already making the code harder to navigate.

I followed mostly the same flow for all other features added.

## Review and refactoring

Before submission, I checked the code in each layer against the conventions I had chosen.

- Moved the API and UI under `Presentation/` to make their shared role clear.
- Moved data access into Infrastructure, where the database implementation belongs.
- Moved storage constraints out of the domain entities and into the persistence layer.
- Simplified the application exceptions so that each error no longer needed its own class.
- Classified `OrderTotals` as a value object because it has no identity or independent lifecycle.
- Corrected inconsistent HTTP status codes returned when an order status update failed.

## Testing

During user acceptance testing, I checked that the order list showed the newest orders first and that all columns displayed the expected values. I also tried to create an order with an existing reference and checked that the duplicate was blocked with a helpful message.

I added NUnit unit and integration tests for the backend business rules I considered most important to the assignment.
