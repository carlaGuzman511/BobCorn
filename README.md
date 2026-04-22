# Bob's Corn - Rate Limiter App

## 1. Video Link

>

## 2. Overview

Bob’s Corn is a full-stack application that enforces a business rule:

> **A client can only buy 1 corn per minute**

---

## 3. Architecture

### 3.a Backend

* Clean Architecture (Application, Domain, Infrastructure, API)
* Dapper for data access
* SQL Server for persistence

### 3.b Frontend

* Angular (standalone components)
* Reactive UI with cooldown timer

---

## 4. Features

* Rate limiting per client
* Cooldown feedback (frontend)
* Retry-After header support
* Purchase history tracking

---

## 5. Testing

* Unit tests with xUnit and Moq
* Controller tests (HTTP behavior)
* Edge cases covered (rate limit, headers)

---

## 6. How to Run

### Backend

requirements:
* SDK .NET 8

```bash
cd BobCorn.API
dotnet run
```

### Frontend

requirements:

* Angular CLI: 19.2.13
* Node: 24.11.1
* Package Manager: bun 1.2.9

```bash
cd bob-corn-web-app
npm install
npm run start
```

---

## 7. Database (SQL Server)

Create database:

```sql
CREATE DATABASE BobCorn;
```

Create tables:

```sql
CREATE TABLE CornPurchases (
    Id UNIQUEIDENTIFIER PRIMARY KEY,
    ClientId NVARCHAR(100) NOT NULL,
    PurchasedAt DATETIMEOFFSET NOT NULL
);

CREATE INDEX IX_CornPurchases_ClientId_PurchasedAt
ON CornPurchases (ClientId, PurchasedAt DESC);

CREATE TABLE ClientPurchaseState (
    ClientId NVARCHAR(100) PRIMARY KEY,
    LastPurchaseAt DATETIMEOFFSET NOT NULL
);
```

![Database Tables](images/db-tables.png)

---

## 8. Postman Collection Requests
---

Postman 200 OK

![Postman 200 OK](images/postman-200.png)

Postman 429 Too Many Requests

![Postman 429 Too Many Requests](images/postman-429.png)

---

## 9. Design Decisions

* Separate read model (`ClientPurchaseState`) for performance
* Use Dapper for lightweight data access
* Frontend handles UX, backend enforces rules
* Clean separation of concerns

---


## 10. Code Challenge Reference:

- https://coda.io/d/Shared-Software-Engineer-Challenge_dyBwZvKLrdE/Challenge-Software-Engineer_suydMA8O#_luFSdSnr
