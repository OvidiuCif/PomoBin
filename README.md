# PomoBin 🍅

A wannabe Pomodoro timer built in .NET 10, structured as a three-project solution.  
The desktop app runs standalone; the REST API exposes the same timer for external control or future integrations.

---

## Solution structure

```
PomoBin.slnx
├── PomoApp.Core/          # Shared logic — no UI, no framework dependencies
│   ├── Settings.cs
│   └── TimerService.cs
│
├── PomoApp.Api/           # ASP.NET Core 10 minimal-API — HTTP control surface
│   ├── Program.cs
│   ├── appsettings.json
│   ├── appsettings.Development.json
│   └── Properties/launchSettings.json
│
└── PomoApp.Desktop/       # WPF .NET 10 — primary user interface
    ├── App.xaml / App.xaml.cs
    ├── MainWindow.xaml / MainWindow.xaml.cs
    └── AssemblyInfo.cs
```

---

## Project responsibilities

### `PomoApp.Core`
Framework-agnostic class library. Everything else depends on this; it depends on nothing.

| Type | Purpose |
|---|---|
| `Settings` | Holds configurable durations: work (25 min), short break (5 min), long break (15 min), sessions before long break (4) |
| `PomodoroTimerService` | Drives the countdown using `Task.Delay` on a background thread. Raises `Tick`, `PhaseChanged`, and `SessionCompleted` events. |
| `PomodoroPhase` | Enum: `Work` · `Break` · `LongBreak` |

**Phase progression:**

```
Work → Break → Work → Break → Work → Break → Work → LongBreak → (repeat)
  1              2              3              4
```
A long break is triggered every `SessionsBeforeLongBreak` *completed* work sessions.

---

### `PomoApp.Desktop`
WPF application. Creates its own `PomodoroTimerService` instance directly from `PomoApp.Core`.  
All timer callbacks are marshalled back to the UI thread via `Dispatcher.Invoke`.

**Controls:**
| Button | Action |
|---|---|
| Start / Pause | Toggles `timer.Start()` / `timer.Pause()` |
| Reset | Resets phase, session count, and remaining time to defaults |
| Skip → | Advances to the next phase immediately (does not count as a completed session) |

> The desktop app is **fully self-contained** — it does not talk to the API.

---

### `PomoApp.Api`
ASP.NET Core 10 minimal API. Registers `Settings` and `PomodoroTimerService` as singletons in DI.  
This is a **separate timer instance** from the desktop app — useful for scripting, dashboards, or a future web front-end.

| Method | Route | Description |
|---|---|---|
| `GET` | `/timer/status` | Returns `isRunning`, `phase`, `remaining`, `completedSessions` |
| `POST` | `/timer/start` | Starts the timer |
| `POST` | `/timer/pause` | Pauses the timer |
| `POST` | `/timer/reset` | Resets to initial Work state |
| `POST` | `/timer/skip` | Skips to the next phase |
| `GET` | `/settings` | Returns current settings |
| `PUT` | `/settings` | Updates all setting values |

OpenAPI schema available at `/openapi/v1.json` when running in `Development`.

---

## Tech stack

| Layer | Technology |
|---|---|
| Language | C# 13 / .NET 10 |
| Desktop UI | WPF (Windows only) |
| API | ASP.NET Core 10 minimal APIs |
| Solution format | `.slnx` (Visual Studio 2026) |
| OpenAPI | `Microsoft.AspNetCore.OpenApi` 10.x |
