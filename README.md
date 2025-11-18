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

This project uses BaseArchitecture as a git submodule, providing the core framework while keeping game-specific code separate.

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

## 🛠️ Using as Reference

### Junction Points Setup (Windows)
The project uses Windows Junction Points to link the submodule. Run these commands from the project root directory:
```powershell
New-Item -ItemType Junction -Path "Assets\Submodules\BaseArchitecture\Scripts" -Target "$PWD\Submodules\BaseArchitecture\Assets\Scripts\Core"
New-Item -ItemType Junction -Path "Assets\Submodules\BaseArchitecture\UI" -Target "$PWD\Submodules\BaseArchitecture\Assets\UI"
```

Or use absolute paths:
```powershell
New-Item -ItemType Junction -Path "D:\YourPath\SpaceInvaders\Assets\Submodules\BaseArchitecture\Scripts" -Target "D:\YourPath\SpaceInvaders\Submodules\BaseArchitecture\Assets\Scripts\Core"
New-Item -ItemType Junction -Path "D:\YourPath\SpaceInvaders\Assets\Submodules\BaseArchitecture\UI" -Target "D:\YourPath\SpaceInvaders\Submodules\BaseArchitecture\Assets\UI"
```

### Assembly Definitions
- `SpaceInvaders.asmdef` references:
  - `BaseArchitecture.Core.asmdef`
  - `Zenject`, `UniTask`, `DOTween`, `TextMeshPro`

---

## 🚀 Getting Started

1. **Clone the repository with submodules**:
   ```bash
   git clone --recurse-submodules https://github.com/TheodorMihail/SpaceInvaders.git
   ```

2. **Set up junction points** (Windows only - see above)

3. **Open in Unity** (2022.3+ recommended)

4. **Install dependencies**:
   - Zenject
   - UniTask
   - DOTween
   - Addressables
   - TextMeshPro

5. **Open the Preload scene** and press Play
