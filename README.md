# Space Invaders

### A modern Unity recreation of the classic Space Invaders arcade game — built with scalable architecture and best practices from the ground up.

---

## 🧱 Overview

This project demonstrates clean Unity architecture using the **[BaseArchitecture](https://github.com/TheodorMihail/BaseArchitecture)** framework. It brings together:

- ✅ **SOLID principles** — to ensure clear responsibilities and maintainable code
- 🎮 **MVC pattern** — with Screens and HUDs for flexible UI management
- 🔁 **State Machines** — for predictable, extensible gameplay flow
- 🧠 **Zenject (Dependency Injection)** — for decoupled, testable systems
- 🔄 **UniTask** — for clean async/await operations
- 📦 **Addressables** — for efficient asset management
- ♻️ **Object Pooling** — for optimized enemy and projectile reuse
- 🗂️ **Repository Pattern** — for centralized configuration and data management
- 📬 **Message Bus** — for decoupled pub/sub communication between systems
- ⚡ **Assembly Definitions** — for faster compile times
- 🧪 **Comprehensive test coverage** — with EditMode and PlayMode tests

---

## 📦 Why This Matters

Space Invaders serves as a **reference implementation** of the BaseArchitecture framework, demonstrating how to structure a real game with industry best practices. The architecture scales from simple arcade games to complex multiplayer titles.

---

## 🏗️ Architecture

> **For detailed architecture documentation**, see the **[BaseArchitecture README](https://github.com/TheodorMihail/BaseArchitecture#-architecture-guide)**.

This project consumes BaseArchitecture as a **UPM package** (via Git URL), providing the core framework while keeping game-specific code cleanly separated.

### 🎮 Game-Specific Implementation

**State Management**
- Game flow controlled through `GameplayState` and `GameOverState`
- Custom session lifecycle system for gameplay restarts without scene reloads

**Core Systems**
- **Managers** — Level progression, player/enemy lifecycle, camera setup
- **Services** — Spawn service (factory), input handling
- **Components** — Interface-driven spaceship hierarchy, projectiles, collision detection

**Data Management**
- ScriptableObject-based configurations accessed via Repository Pattern
- Object pooling for enemies and projectiles

### 🧪 Testing

Comprehensive test coverage using Unity Test Framework with NSubstitute and Zenject:

- **EditMode** — LevelManager, GameStateMachine
- **PlayMode** — EnemiesManager, PlayerManager (async/UniTask operations)

Interface-based design enables full testability of all game systems.

---

## 📦 Installing BaseArchitecture

To use BaseArchitecture in your own project, add to `Packages/manifest.json`:

```json
{
  "scopedRegistries": [
    {
      "name": "OpenUPM",
      "url": "https://package.openupm.com",
      "scopes": [
        "com.svermeulen.extenject",
        "com.cysharp.unitask",
        "com.neuecc.unirx"
      ]
    }
  ],
  "dependencies": {
    "com.theodormihail.basearchitecture": "https://github.com/TheodorMihail/BaseArchitecture.git?path=Assets/Package"
  },
  "testables": [ "com.svermeulen.extenject" ]
}
```

The scoped registry resolves the package's dependencies (Zenject, UniTask, UniRx) from OpenUPM. `testables` enables Zenject's test fixtures. **DOTween** must be imported manually from the [Asset Store](http://dotween.demigiant.com/).

---
