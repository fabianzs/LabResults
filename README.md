# 🏥 LabResults Data Processing System

This solution processes laboratory data from pipe-delimited flat files, validates the structure, and persists the results into a SQLite database. It then exposes the stored patient and test results via a RESTful Web API.

## 🏗️ Architecture and Project Structure

The solution follows a **Clean Architecture** (or **Layered Architecture**) pattern, organized into four primary projects:

| Project | Layer | Responsibilities |
| :--- | :--- | :--- |
| **`LabResults.Domain`** | Contains all **business logic**, **entities** (`Patient.cs`, `Sample.cs`, `TestResult.cs`), DTOs (`Models`), interfaces (`IPatientReader`), and custom exceptions (`NotFoundException.cs`). It is framework-agnostic. |
| **`LabResults.DataLoader`** | The **console application** that runs first to perform ETL (Extract, Transform, Load) operations. |
| **`Labresults.Infrastructure`** | Handles external concerns: **Entity Framework Core** setup, database context (`LabResultsDbContext`), persistence logic, and reader/writer implementations. |
| **`LabResults.Web`** | The **API** that exposes data endpoints and handles HTTP concerns (controllers, middleware). |

---

## 🚀 Getting Started

### Data Loader (Console Application)

The `LabResults.DataLoader` project is the entry point for data ingestion and should be run first.

* **Dynamic File Selection:** The application is designed to dynamically select the source data file at runtime.
* **Headers Configuration:** The expected header names (`_expectedHeaders`) for file validation are configured via `IOptions<LabFileSettings>` (defined in `appsettings.json`). The file reader is **order-independent**—it can map fields even if the column order changes, as long as all required headers are present.
* **Persistence:** It uses **Entity Framework Core** with the **SQLite provider** to persist processed data. The location of the SQLite database file (`.db`) is configured in `appsettings.json`.

 **❗ IMPORTANT: Configure Your Location**
 Make sure to configure your own **local file paths** for the SQLite database (in `appsettings.json`) before running the application.

### Web API (RESTful Endpoints)

The `LabResults.Web` project exposes the persisted data.

| Endpoint | Description |
| :--- | :--- |
| `GET /api/patients` | Retrieves a list of all **Patients**. |
| `GET /api/patients/{id}` | Retrieves a single **Patient** record. |
| `GET /api/patients/{patientId}/labresults` | Retrieves all **Test Results** associated with a specific patient. |

#### Error Handling

The API uses the **`ExceptionHandlerMiddleware`** (in `LabResults.Web`) to handle all exceptions thrown from the data layers, ensuring RESTful error responses:

* **404 Not Found:** Returned when a specific resource is requested but not found (triggered by a `NotFoundException` from the Domain).
* **500 Internal Server Error:** Returned for unexpected exceptions, preventing sensitive application details from leaking.

---

## 🧬 Data Model Explanation (Entities)

The core business domain is defined by a cascading relationship that maps one-to-many relationships in the database:

**Patient $\rightarrow$ Samples $\rightarrow$ TestResults**

| Entity | Primary Role | Relationship Assumption |
| :--- | :--- | :--- |
| **`Patient`** | Holds demographic details (ID, Name, DOB). | One **Patient** can have many **Samples**. |
| **`Sample`** | Represents a collected physical specimen (e.g., blood vial). Identified by a unique **Barcode** (`long`), **ClinicNo** (`int`), and Collection Date/Time. | One **Sample** can be used for many **Test Results**. |
| **`TestResult`** | The specific result for one analysis (e.g., Glucose: 95 mg/dL). | Multiple rows in the input file, though distinct, map to a single Sample if they share the same identifying sample information. |

This structure ensures that multiple tests performed on the same specimen are correctly grouped under one `Sample` entity in the database. Even though the flat file provides only one `TestResult` per line for now, I can imagine multiple tests being run on the same sample.
