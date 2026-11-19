CREATE TABLE IF NOT EXISTS Customers (
    Id TEXT NOT NULL PRIMARY KEY,
    Name TEXT NOT NULL CHECK (length(Name) <= 200),
    Email TEXT NOT NULL CHECK (length(Email) <= 254)
) STRICT;

CREATE TABLE IF NOT EXISTS Orders (
    Id TEXT NOT NULL PRIMARY KEY,
    ClientProvidedExternalReference TEXT NOT NULL
        CHECK (length(ClientProvidedExternalReference) <= 128),
    OrderDateTimestamp TEXT NOT NULL,
    CompletedDateTimeStamp TEXT,
    Status INTEGER NOT NULL CHECK (Status IN (0, 1, 2, 3)),
    Currency TEXT NOT NULL CHECK (length(Currency) = 3),
    Notes TEXT NOT NULL,
    CustomerId TEXT NOT NULL REFERENCES Customers (Id),
    CONSTRAINT UQ_Orders_ClientProvidedExternalReference UNIQUE (ClientProvidedExternalReference)
) STRICT;

CREATE INDEX IF NOT EXISTS IX_Orders_CustomerId ON Orders (CustomerId);

CREATE TABLE IF NOT EXISTS LineItems (
    Id TEXT NOT NULL PRIMARY KEY,
    OrderId TEXT NOT NULL REFERENCES Orders (Id) ON DELETE CASCADE,
    Sku TEXT NOT NULL CHECK (length(Sku) <= 64),
    Name TEXT NOT NULL CHECK (length(Name) <= 200),
    Quantity INTEGER NOT NULL CHECK (Quantity > 0),
    -- TEXT preserves .NET decimal prices without rounding through SQLite REAL.
    UnitPrice TEXT NOT NULL
) STRICT;

CREATE INDEX IF NOT EXISTS IX_LineItems_OrderId ON LineItems (OrderId);
