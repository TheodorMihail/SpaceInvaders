# Space Invaders

### A modern Unity recreation of the classic Space Invaders arcade game — built with scalable architecture and best practices from the ground up.

---

## 🧱 Overview

This project demonstrates clean Unity architecture using the **[BaseArchitecture](https://github.com/TheodorMihail/BaseArchitecture)** framework:

- ✅ **SOLID principles** for maintainable code
- 🎮 **MVC pattern** with Screens and HUDs
- 🔁 **State Machines** for game flow
- 🧠 **Zenject (Dependency Injection)** for decoupling
- 🔄 **UniTask** for async operations
- 📦 **Addressables** for asset management
- ⚡ **Assembly Definitions** for fast compilation

---

## 📦 Why This Matters

Space Invaders serves as a **reference implementation** of the BaseArchitecture framework, demonstrating how to structure a real game with industry best practices. The architecture scales from simple arcade games to complex multiplayer titles.

---

## 🏗️ Architecture

> **For detailed architecture documentation**, see the **[BaseArchitecture README](https://github.com/TheodorMihail/BaseArchitecture#-architecture-guide)**.

This project uses BaseArchitecture as a git submodule, providing the core framework while keeping game-specific code separate.

### 🎮 Game-Specific Implementation

**Architectural Patterns**
- **Repository Pattern** — `RepositoryManager` centralizes access to ScriptableObject configurations
- **Object Pooling** — Enemies and projectiles reuse pooled instances for performance
- **State Machine** — `GameplayState` and `GameOverState` control game flow
- **Message Bus** — Decoupled pub/sub communication for game events
- **Component-Based Design** — Reusable components with inheritance-based behavior

**Session Lifecycle**
- Custom lifecycle system (`IGameInitializeListener`, `IGameStartedListener`, `IGameEndedListener`)
- Enables gameplay restarts without scene reloads
- Separates DI container setup from game session management

**Key Systems**
- **Managers** — LevelManager, PlayerManager, EnemiesManager, CameraManager
- **Services** — SpawnService (factory), InputService
- **Components** — SpaceshipBehaviourComponent hierarchy, ProjectileBehaviourComponent, CollisionDetectionComponent

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
