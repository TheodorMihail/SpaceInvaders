# Space Invaders

### A modern Unity recreation of the classic Space Invaders arcade game, built with scalable architecture and best practices from the ground up.

---

## 🧱 Overview

This project demonstrates clean Unity architecture using the **[BaseArchitecture](https://github.com/TheodorMihail/BaseArchitecture)** framework. It brings together:

- ✅ **SOLID principles**: to ensure clear responsibilities and maintainable code
- 🎮 **MVC pattern**: with Screens and HUDs for flexible UI management
- 🔁 **State Machines**: for predictable, extensible gameplay flow
- 🧠 **Zenject (Dependency Injection)**: for decoupled, testable systems
- 🔄 **UniTask**: for clean async/await operations
- 📦 **Addressables**: for efficient asset management
- ♻️ **Object Pooling**: for optimized enemy and projectile reuse
- 🗂️ **Repository Pattern**: for centralized configuration and data management
- 📬 **Message Bus**: for decoupled pub/sub communication between systems
- 🔊 **Audio System**: channel-based music/SFX playback with crossfading and persisted volume settings
- ⚡ **Assembly Definitions**: for faster compile times
- 🧪 **Comprehensive test coverage**: with EditMode and PlayMode tests

---

## 📦 Why This Matters

Space Invaders serves as a **reference implementation** of the BaseArchitecture framework, demonstrating how to structure a real game with industry best practices, and how the same structure holds as a project grows.

---

## 🏗️ Architecture

> **For detailed architecture documentation**, see the **[BaseArchitecture README](https://github.com/TheodorMihail/BaseArchitecture#-architecture-guide)**.

This project consumes BaseArchitecture as a **UPM package** (via Git URL), providing the core framework while keeping game-specific code cleanly separated.

### 🧭 Important Notes

These hold across the whole codebase, and everything below depends on them:

- **No singletons, no static state and no dependency lookups inside the scene.** Every dependency is injected, so nothing has hidden coupling to scene layout
- **Facts are past-tense messages on the message bus**, so a publisher never knows who reacts to it. Commands and queries are direct calls on injected interfaces; C# events cover the direct links with few subscribers
- **Managers are split by scope.** Project-scoped ones persist across scenes, scene-scoped ones are bound to their own state machine
- **A manager's services are private to it.** Each owner binds its helpers in its own DI subcontainer and drives their lifecycle, so a service is reachable only through the manager that owns it. Encapsulation is enforced by the container, not by convention
- **Interfaces throughout**, so every system is substitutable, including under test

### 🎮 Game-Specific Implementation

**State Management**
- Game flow controlled through `GameplayState` and `GameOverState`
- Custom session lifecycle system for gameplay restarts without scene reloads

**Player Progression**
- **Talents**: permanent, currency-purchased stat upgrades, authored per-level as flat or percentage bonuses
- **Equippable items**: rarity-tiered loot with randomly-rolled stat affixes, dropped on enemy kills and equipped across ship slots from a dedicated Inventory screen
- **Loot system**: a single weighted roll per enemy kill decides whether a powerup, an item, or nothing drops
- **Salvage**: unwanted loot sells back for currency at a per-rarity rate
- **Powerups**: timed or instant pickups applied as temporary `ShipStats` bonuses, with their own HUD indicator/timer
- A shared flat/percentage stat-bonus model (`ShipStats`) used consistently by talents, items, and powerups

**Level & Combat Structure**
- Levels are composed of multiple enemy waves, laid out from a set of procedural formation templates, with boss waves flagged separately and announced via a dedicated HUD callout
- Waves are formed of different enemy types, each with their own behaviour and stats, including ones that break apart into smaller ships when destroyed
- **Environmental hazards**: obstacles that cross the play area alongside the waves, authored per wave so each level sets its own mix and frequency
- Boss enemies switch between several shooting behaviours and can call in reinforcements as they lose health, with health broadcast over the message bus to a dedicated boss health bar
- A star rating is awarded per level based on damage taken versus per-level thresholds, gating progression to the next level

**Core Systems**
- **Managers**: own the vital concerns, such as progression, currency, inventory, spawning, input, time and the level session
- **Services**: focused helpers owned outright by one manager, such as drop rolls, score accumulation, hazard cadence and sound wiring
- **Components**: interface-driven spaceship hierarchy, projectiles, collision detection, pooled VFX and world-space health bars

**Data Management**
- ScriptableObject-based configurations for almost every in-game entity, accessed via Repository Pattern
- Object pooling for frequently spawned entities
- Progression and settings persisted via BaseArchitecture's `IPersistenceManager`

**Developer Tools**
- Level Generator editor window (`SpaceInvaders > Level Generator`) for authoring level configs, including a formation-template generator and a custom inspector showing each level's generator seed
- Animation tools (`SpaceInvaders > Animations`) for turning a sliced sprite sheet into looping clips and their controllers
- Keyboard shortcuts for quickly creating new config assets in the selected folder
- Custom spaceship inspector that live-displays runtime `ShipStats` while playing
- Debug shortcuts for granting/clearing progression while testing, gated to the Editor and development builds

### 🧪 Testing

Comprehensive test coverage using Unity Test Framework with NSubstitute and Zenject:

- **EditMode**: the managers, state machines, repositories and stat calculations
- **PlayMode**: the systems that only make sense with a running loop, including async/UniTask operations

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
    "com.theodormihail.basearchitecture": "https://github.com/TheodorMihail/BaseArchitecture.git?path=Assets/UnityPackages/BaseArchitecture#v1.5.1"
  },
  "testables": [ "com.svermeulen.extenject" ]
}
```

The `#v1.5.1` pins the version, matching this project's own manifest. Bump it to pull a newer [release tag](https://github.com/TheodorMihail/BaseArchitecture/tags). The scoped registry resolves the package's dependencies (Zenject, UniTask) from OpenUPM. `testables` enables Zenject's test fixtures. See the [BaseArchitecture README](https://github.com/TheodorMihail/BaseArchitecture#-installation) for its own requirements.

---

## 📄 License

All rights reserved, see [LICENSE](LICENSE). This repository is public for portfolio/viewing purposes only; no reuse or redistribution is permitted without written permission.

### Third-party materials

The art, models, audio and plugins under `Assets/GameAssets/2D/External`, `Assets/GameAssets/3D/External`, `Assets/GameAssets/Sounds/External` and `Assets/Plugins` are third-party materials. They remain the property of their respective owners, are governed by their own licenses, and are not covered by the notice above. The same applies to BaseArchitecture, Zenject and UniTask, which resolve through the Package Manager.

---
