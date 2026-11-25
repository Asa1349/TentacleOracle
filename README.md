# TentacleOracle

**TentacleOracle** is an intelligent web application designed to eliminate the daily stress of meal planning. The project is currently being transitioned from a classic Windows desktop application—which I developed entirely from scratch—into a modern, cloud-ready **Blazor WebAssembly** architecture.

---

## Core Features

* **Meal Management**: A comprehensive CRUD system (Create, Read, Update, Delete) for managing individual dishes, including metadata such as category, cooking effort, and personal rating.
* **Intelligent Week Planner**: A generation function that provides a 7-day meal plan with a single click.
* **Customization**: The generated suggestions are flexible; users can manually swap or edit specific dishes within the generated list to suit their needs.
* **Full-Stack Development**: Every aspect of this project, including the frontend, backend, and the UI/UX design, is self-developed.

---

## The Selection Algorithm

The heart of the application is a custom selection algorithm that goes beyond simple randomization:

* **Weighted Rating**: Dishes with higher user ratings have a statistically higher probability of being selected. This ensures that favorite meals appear more frequently while maintaining variety.
* **Effort Balancing**: The algorithm accounts for preparation time by limiting high-effort dishes to a maximum of two per week, ensuring the plan remains practical for a busy work week while reserving more complex cooking for the weekend.

---

## Technical Stack & Migration

* **Frontend**: Blazor WebAssembly (.NET)
* **Backend**: ASP.NET Core Web API
* **Design**: Custom CSS and UI components
* **Data Persistence**: SQL Database / Entity Framework Core

> This project serves as a showcase for migrating a self-authored legacy desktop environment to a modern web-based ecosystem, demonstrating proficiency in algorithm design, system architecture, and UI/UX consistency.

---

## Unit Tests

`TentacleOracle.Tests` uses NUnit and Moq and mirrors the application's `BusinessLogic`
and `Services` folders. Run all tests from the solution directory:

```shell
dotnet test TentacleOracle.sln
```

The tests also appear in Rider's Unit Tests window. No running application or browser
is required. Persistence tests use an isolated SQLite database held in memory.

* `TestData` contains shared cases, a dish factory, a strict-Moq meal-plan context, and the SQLite test database.
* Related cases use `TestCase` or `TestCaseSource`; shared sources are reused across test classes.
* Random selection is tested by count, membership, uniqueness, and unchanged input, not a particular order.
* Cancellation tests control asynchronous completion with Moq, without sleeps.
* `DishService` and `SettingsService` run against real EF Core/SQLite persistence because mocking EF internals would not verify database behavior.

Razor rendering is not covered; component tests can be added separately.

---

## Future Roadmap

The FoodOracle is actively being expanded with the following features:

* **Recipe Integration**: Adding detailed cooking instructions and ingredients to each dish.
* **Interactive Dashboard**: A weekly overview featuring drag-and-drop functionality to assign dishes to specific weekdays.
* **Nutrition Scoring**: Categorization based on health metrics, including Low-Carb and High-Protein tags.
* **Automated Shopping List**: Generation of a consolidated shopping list based on the finalized weekly plan.

---

## Intellectual Property and Licensing

**Copyright © Alexandra Pöbl 2026. All rights reserved.**

Please note that this repository is for **demonstration and portfolio purposes only**. The source code, logic, and design are my original intellectual property. No part of this project may be copied, redistributed, or used in other projects without my explicit written permission.
