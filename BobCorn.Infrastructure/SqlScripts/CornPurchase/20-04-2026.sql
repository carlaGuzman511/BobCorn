CREATE TABLE CornPurchases (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClientId NVARCHAR(100) NOT NULL,
    PurchasedAt DATETIMEOFFSET NOT NULL
);

CREATE INDEX IX_CornPurchases_ClientId_PurchasedAt
ON CornPurchases (ClientId, PurchasedAt DESC);