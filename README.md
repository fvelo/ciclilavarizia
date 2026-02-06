# 🚲 Ciclilavarizia Backend

Welcome to the core of the **Ciclilavarizia** ecosystem. This is a high-performance, domain-driven API designed to manage complex Sales Order lifecycles with strict data integrity and a focus on clean architecture.

## 📝 Overview

**Ciclilavarizia** is a backend solution built to handle the intricacies of an inventory and sales platform. 

---

## 🛠 The Tech Stack

| Component | Technology | Reasoning |
| :--- | :--- | :--- |
| **Runtime** | **.NET 8.0** | Current industry standard for cross-platform enterprise performance. |
| **Framework** | **ASP.NET Core Web API** | Robust middleware, automatic model validation, and dependency injection. |
| **ORM** | **Entity Framework Core** | Advanced LINQ capabilities and built-in transaction management. |
| **Database** | **SQL Server** | Reliable relational storage optimized for complex join operations. |
| **Validation** | **DataAnnotations** | Decoupled validation logic that runs at the Model Binding layer. |
| **Patterns** | **Result & DTO Pattern** | Ensures consistent API responses and prevents Over-Posting security risks. |

---

## 🚀 Key Architectural Features

### 1. Controller APIs
The choice of using Controller APIs, to build the REST APIs, is made as to use the major convenience that the [ControllerApi] attribute crate. Such as automatic ModelValidation, automatic ProblemDetails creation and ModelBinding convinience without using [From*] Attibutes.

### 2. Secure DTO Separation
We utilize a strict separation between **Command DTOs** (for `POST`/`PUT`) and **View DTOs** (for `GET`). 
* **Security:** Prevents clients from manually setting read-only fields like `SalesOrderId` or `TotalDue`.
* **Clarity:** The API contract clearly defines exactly what data is required and what is returned.

### 3. EF Core and ADO.NET
The project uses three different DBs. Two are Microsoft SQL Server: the first used to store sensible data (passwords, emails, salts, backand errors) managed with ADO.NET as to have the most granular control over the access, the second to store all the other data (orders, customer details) managed through ORM as to simplify the code. The third db is a MongoDB (No-SQL) to store the carts managed through ORM.

### 3. Integrated Global Exception Handling
The project features a centralized exception middleware. This allows services to `throw` exceptions during critical failures, which are then caught, logged, and returned to the client as a clean, standardized JSON error response without leaking sensitive stack traces.


---

## 🚦 Getting Started

### Prerequisites
* **.NET SDK 8.0+**
* **SQL Server** (LocalDB or Express)

### Installation

1. **Clone the repository:**
   ```bash
   git clone [https://github.com/YourUsername/Ciclilavarizia.git](https://github.com/YourUsername/Ciclilavarizia.git)
   cd Ciclilavarizia