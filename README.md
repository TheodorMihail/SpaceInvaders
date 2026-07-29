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
- 🔊 **Audio System** — channel-based music/SFX playback with crossfading and persisted volume settings
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

**Player Progression**
- **Talents** — permanent, currency-purchased stat upgrades, authored per-level as flat or percentage bonuses
- **Equippable items** — rarity-tiered loot with randomly-rolled stat affixes, dropped on enemy kills and equipped across ship slots from a dedicated Inventory screen
- **Loot system** — a single weighted roll per enemy kill decides whether a powerup, an item, or nothing drops, so a kill can never grant more than one reward
- **Powerups** — timed or instant pickups (Invincibility, Heal, Damage Boost, Rapid Fire, Spread Shot) applied as temporary `ShipStats` bonuses, with their own HUD indicator/timer
- A shared flat/percentage stat-bonus model (`ShipStats`) used consistently by talents, items, and powerups, with a guaranteed floor so no stacked malus can zero out or invert a stat

**Level & Combat Structure**
- Levels are composed of multiple enemy waves (procedurally-templated formations — Grid, V-Shape, Line, Diamond, Circle, Cluster), with boss waves flagged separately and announced via a dedicated HUD callout
- Boss enemies alternate between an aimed shot and a multi-bullet spread attack, with health broadcast over the message bus to a dedicated boss health bar
- A 1–3 star rating is awarded per level based on cumulative damage taken versus per-level thresholds, gating progression to the next level

**Core Systems**
- **Managers** — Level progression, currency, talents, inventory, equipment, powerups, player/enemy lifecycle, camera bounds, platform detection (touch vs. desktop, frame rate)
- **Services** — Spawn service (factory), input handling, score tracking, level/wave session flow, sound wiring (`SoundsService` translates message-bus events into `SoundsManager` playback)
- **Components** — Interface-driven spaceship hierarchy, projectiles, collision detection, pooled VFX, world-space health bars

**Data Management**
- ScriptableObject-based configurations accessed via Repository Pattern
- Object pooling for enemies, projectiles, and item/powerup pickups
- Progression (levels/stars, talents, currency, inventory, equipment) and audio settings persisted via BaseArchitecture's `IPersistenceManager`

**Developer Tools**
- Level Generator editor window (`SpaceInvaders > Level Generator`) for authoring level configs, including a formation-template generator and a custom inspector showing each level's generator seed
- Keyboard shortcut (`Ctrl+Shift+I`) for quickly creating new Item Config assets in the selected folder
- Custom spaceship inspector that live-displays runtime `ShipStats` (health, speed, fire rate, damage, invincibility, etc.) while playing
- Editor/Development-Build-only debug shortcuts (F1–F4, F9–F12) for granting/clearing progression while testing

### 🧪 Testing

Comprehensive test coverage using Unity Test Framework with NSubstitute and Zenject:

- **EditMode** — LevelManager, GameStateMachine, RepositoryManager, EquipmentManager, LootManager, InventoryManager, ShipStats
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
        "com.cysharp.unitask"
      ]
    }
  ],
  "dependencies": {
    "com.theodormihail.basearchitecture": "https://github.com/TheodorMihail/BaseArchitecture.git?path=Assets/UnityPackages/BaseArchitecture#v1.0.0"
  },
  "testables": [ "com.svermeulen.extenject" ]
}
```

The `#v1.0.0` pins the version — bump it to pull a newer [release tag](https://github.com/TheodorMihail/BaseArchitecture/tags). The scoped registry resolves the package's dependencies (Zenject, UniTask) from OpenUPM. `testables` enables Zenject's test fixtures. **DOTween** must be imported manually from the [Asset Store](http://dotween.demigiant.com/).

---

## 📄 License

All rights reserved — see [LICENSE](LICENSE). This repository is public for portfolio/viewing purposes only; no reuse or redistribution is permitted without written permission.

---
