INSERT INTO Customers (Id, Name, Email) VALUES
    ('10000000-0000-4000-8000-000000000001', 'Amina Patel', 'amina@karoo-coffee.example'),
    ('10000000-0000-4000-8000-000000000002', 'Lebo Mokoena', 'lebo@moonrise-club.example'),
    ('10000000-0000-4000-8000-000000000003', 'Nandi Jacobs', 'nandi@velvet-thunder.example'),
    ('10000000-0000-4000-8000-000000000004', 'Pieter van Wyk', 'pieter@pigeon-parade.example'),
    ('10000000-0000-4000-8000-000000000005', 'Zinhle Nkosi', 'zinhle@sea-glass-studio.example'),
    ('10000000-0000-4000-8000-000000000006', 'Samira Desai', 'samira@tiny-dinosaur-lab.example'),
    ('10000000-0000-4000-8000-000000000007', 'Thabo Ndlovu', 'thabo@orbit-arcade.example'),
    ('10000000-0000-4000-8000-000000000008', 'Isabel Daniels', 'isabel@fable-picnic.example');

-- Spread the orders between the beginning of the current UTC month and startup.
WITH DemoOrders (OrderNumber, Reference, CustomerNumber, Status, Notes) AS (
    VALUES
        (1, 'DEMO-KAROO-01', 1, @Fulfilled, 'Supplies for a sunrise coffee pop-up in the Karoo.'),
        (2, 'DEMO-MOON-02', 2, @Confirmed, 'Rooftop stargazing kit. Keep the lights red.'),
        (3, 'DEMO-VELVET-03', 3, @Pending, 'Parts for a cardboard robot costume with a velvet cape.'),
        (4, 'DEMO-PIGEON-04', 4, @Cancelled, 'The parade changed its mascot from a pigeon to a pelican.'),
        (5, 'DEMO-TIDE-05', 5, @Fulfilled, 'Workshop supplies for a sea-glass mosaic night.'),
        (6, 'DEMO-DINO-06', 6, @Confirmed, 'Miniature fossil digs for a school science fair.'),
        (7, 'DEMO-ORBIT-07', 7, @Pending, 'Retro arcade repair supplies. Ask for the high-score initials.'),
        (8, 'DEMO-FABLE-08', 8, @Fulfilled, 'A picnic order for a silent reading club.'),
        (9, 'DEMO-MOON-09', 2, @Cancelled, 'Outdoor screening cancelled after the wind forecast.'),
        (10, 'DEMO-KAROO-10', 1, @Pending, 'One golden trophy for the latte-art champion.')
),
SeedClock (MonthStart, ElapsedSeconds) AS (
    SELECT unixepoch('now', 'start of month'), unixepoch('now') - unixepoch('now', 'start of month')
)
INSERT INTO Orders (
    Id, ClientProvidedExternalReference, OrderDateTimestamp, CompletedDateTimeStamp,
    Status, Currency, Notes, CustomerId
)
SELECT
    printf('20000000-0000-4000-8000-%012d', OrderNumber),
    Reference,
    datetime(MonthStart + ElapsedSeconds * OrderNumber / 11, 'unixepoch'),
    CASE WHEN Status = @Fulfilled THEN
        datetime(MonthStart + ElapsedSeconds * (2 * OrderNumber + 1) / 22, 'unixepoch')
    END,
    Status,
    'ZAR',
    Notes,
    printf('10000000-0000-4000-8000-%012d', CustomerNumber)
FROM DemoOrders
CROSS JOIN SeedClock;

WITH DemoItems (OrderNumber, LineNumber, Sku, Name, Quantity, UnitPrice) AS (
    VALUES
        (1, 1, 'CAF-ROAST', 'Karoo sunrise espresso blend', 3, '259.90'),
        (1, 2, 'CAF-FILTER', 'Fold-flat brass coffee dripper', 2, '429.00'),
        (1, 3, 'CAF-CUP', 'Speckled enamel tasting cup', 12, '85.50'),
        (1, 4, 'CAF-CARD', 'Unreasonably specific tasting cards', 4, '48.00'),
        (2, 1, 'AST-MAP', 'Southern sky pocket atlas', 5, '179.95'),
        (2, 2, 'AST-LAMP', 'Red-light headlamp', 3, '224.50'),
        (2, 3, 'AST-LENS', 'Wide-angle telescope eyepiece', 1, '975.00'),
        (3, 1, 'BOT-LED', 'Blinking robot eye kit', 2, '165.00'),
        (3, 2, 'BOT-CAPE', 'Midnight-purple velvet cape', 1, '395.99'),
        (4, 1, 'PAR-FEATHER', 'Silver-grey craft feathers', 6, '42.50'),
        (4, 2, 'PAR-WING', 'Cardboard wing frames', 4, '98.00'),
        (4, 3, 'PAR-KAZOO', 'Assorted parade kazoos', 20, '24.95'),
        (5, 1, 'MOS-GLASS', 'Storm-blue sea-glass tiles', 8, '112.50'),
        (5, 2, 'MOS-GROUT', 'Pearl-white mosaic grout', 3, '95.00'),
        (5, 3, 'MOS-TRAY', 'Hexagonal bamboo tray', 10, '128.00'),
        (5, 4, 'MOS-NIPPER', 'Precision tile nippers', 2, '219.90'),
        (5, 5, 'MOS-TWEEZE', 'Fine-point mosaic tweezers', 5, '68.40'),
        (6, 1, 'DIN-DIG', 'Pocket triceratops excavation kit', 24, '69.95'),
        (6, 2, 'DIN-LABEL', 'Junior paleontologist badges', 30, '18.00'),
        (7, 1, 'ARC-BUTTON', 'Translucent cherry arcade button', 8, '79.50'),
        (7, 2, 'ARC-STICK', 'Eight-way arcade joystick', 2, '395.00'),
        (7, 3, 'ARC-STRIP', 'Neon lavender cabinet light', 3, '210.00'),
        (7, 4, 'ARC-COIN', 'Orbit Arcade token pack', 5, '139.00'),
        (8, 1, 'PIC-RUG', 'Foldable cloud-print picnic rug', 2, '547.50'),
        (8, 2, 'PIC-THERM', 'Library-green vacuum flask', 4, '320.00'),
        (9, 1, 'FIL-SCREEN', 'Inflatable cinema screen', 1, '2450.00'),
        (9, 2, 'FIL-ROPE', 'Reflective guy rope set', 4, '112.00'),
        (9, 3, 'FIL-CONE', 'Midnight popcorn cone pack', 6, '55.50'),
        (10, 1, 'CAF-TROPHY', 'Golden milk-foam rosette trophy', 1, '489.95')
)
INSERT INTO LineItems (Id, OrderId, Sku, Name, Quantity, UnitPrice)
SELECT
    printf('30000000-0000-4000-8000-%012d', OrderNumber * 10 + LineNumber),
    printf('20000000-0000-4000-8000-%012d', OrderNumber),
    Sku, Name, Quantity, UnitPrice
FROM DemoItems;
