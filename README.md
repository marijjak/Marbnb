<h1 align="center">marbnb</h1>

<p align="center">
  <i>The art of staying.</i><br>
  A luxury accommodation booking web application — guests, hosts &amp; admins, beautifully designed.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/C%23-239120?style=flat&logo=csharp&logoColor=white" alt="C#">
  <img src="https://img.shields.io/badge/ASP.NET%20MVC-5-512BD4?style=flat&logo=dotnet&logoColor=white" alt="ASP.NET MVC 5">
  <img src="https://img.shields.io/badge/.NET%20Framework-4.7.2-512BD4?style=flat" alt=".NET Framework 4.7.2">
  <img src="https://img.shields.io/badge/JavaScript-F7DF1E?style=flat&logo=javascript&logoColor=black" alt="JavaScript">
  <img src="https://img.shields.io/badge/jQuery-0769AD?style=flat&logo=jquery&logoColor=white" alt="jQuery">
  <img src="https://img.shields.io/badge/storage-JSON%20files-C9A35E?style=flat" alt="JSON storage">
</p>


---
## Live Demo
 http://marbnb.runasp.net

## Overview

**marbnb** is a full-stack web application that simulates an online platform for booking accommodation — apartments, hotels, villas, chalets and more. It is built entirely on **ASP.NET MVC 5** with all data persisted in **plain JSON text files** (no database), and ships with a fully **custom dark &amp; gold design** instead of any off-the-shelf template.

The platform supports three kinds of users, each with their own experience:

- **Guests** — browse, search and book stays, manage reservations, and review places they've stayed in.
- **Hosts** — publish and manage their own listings, complete with photo galleries.
- **Administrators** — moderate the whole platform: users, listings, reservations and reviews.

---

## ✨ Features

### For everyone
- Curated home page with a live, **combined search** (name, city, type, price range) and **sorting** (name / price / date posted, ascending &amp; descending)
- Rich listing pages with **photo galleries**, host info, average rating and approved reviews
- Registration &amp; login with session-based authentication

### Guests
- Create reservations with smart validation: guest limits, **date-overlap protection** against approved bookings, automatic price calculation
- Manage reservations, filter by status, cancel (with the 24-hour rule)
- Write, edit and delete reviews — only for stays they have actually completed

### Hosts
- Full **create / edit / delete** of listings with mandatory image upload &amp; multi-photo galleries
- Filter and sort their own listings by availability, price and date

### Administrators
- Manage users (guests &amp; hosts) with **search** by name, date-of-birth range and role, plus sorting
- Approve or cancel reservations, approve or reject reviews
- Edit any listing and manage the catalogue
- Loaded from a configuration file — cannot be created from within the app

> Every deletion in the system is **logical (soft delete)** — records are flagged, never physically removed.

---

## 🛠️ Tech stack

| Layer | Technology |
|-------|------------|
| Language | C# |
| Framework | ASP.NET MVC 5 · .NET Framework 4.7.2 |
| Views | Razor · HTML5 · custom CSS · JavaScript · jQuery |
| Persistence | JSON text files via **Newtonsoft.Json** |
| Auth | Session-based + custom role-authorization filter |

---

## 🏛️ Architecture highlights

- **Generic repository** — a single `JsonStore<T>` handles CRUD, soft-delete filtering, `dd/MM/yyyy` dates and human-readable enums for *every* entity.
- **Facade** — a static `Database` class wires each store to its file, seeds demo data, and loads admins from file.
- **Custom authorization** — a `RoleAuthorize` attribute guards actions by role, reading from the session.
- **Clean MVC separation** — Models, ViewModels, Controllers and Razor Views, with a hand-crafted design system in `marbnb.css`.

```
Models  →  JsonStore<T> + Database (JSON persistence)
              ↑
        Controllers (search, validate, save)
              ↓
        Razor Views + custom CSS  →  what the user sees
```

---

## 🚀 Getting started

> Requires **Visual Studio 2022** with the *ASP.NET and web development* workload and **.NET Framework 4.7.2**.

1. Clone or download the repository
2. Open **`Web.sln`** in Visual Studio 2022
3. Build once — NuGet packages restore automatically
4. Press **F5**

Demo data (users, listings, reservations, reviews) is **seeded automatically** on first run.

### Demo accounts

| Role | Username | Password |
|------|----------|----------|
| Administrator | `admin` | `admin123` |
| Host | `marko` / `jovana` | `marko123` / `jovana123` |
| Guest | `ana` / `nikola` / `milica` | `ana123` / `nikola123` / `milica123` |

---

## 🖼️ Images

All property and location photos used in this project are either **AI-generated** or sourced from **[Unsplash](https://unsplash.com)**, and are included **for demonstration purposes only**.

---

## 📄 About

Built as a university project for **Applied Software Engineering** (Web Programming).
Design, architecture and implementation crafted from scratch — no template UI.

<p align="center"><i>marbnb — stay beyond ordinary.</i></p>
