# Space Invaders

### A modern Unity recreation of the classic Space Invaders arcade game — built with scalable architecture and best practices from the ground up.

---

## 🧱 Overview

Built on a solid architectural foundation:
- ✅ **SOLID principles**
- 🎮 **MVC pattern**
- 🔁 **State Machines**
- 🧠 **Zenject (Dependency Injection)**
- 🔄 **UniTask**
- 📦 **Addressables**

---

## 🧰 Architecture & Structure

This project uses the **[BaseArchitecture](https://github.com/TheodorMihail/BaseArchitecture)** framework as a submodule, providing:

### MVC Pattern
- **Screens** manage the full MVC lifecycle
- **Models** handle game data and business logic
- **Views** are Unity MonoBehaviours for UI/visual elements
- **Controllers** orchestrate Model-View interactions

### Project Structure
- `/Assets/Scripts/` - Game-specific code
- `/Assets/Submodules/BaseArchitecture/` - Symlinked core framework (Scripts, UI)
- `/Submodules/BaseArchitecture/` - Git submodule
- Assembly definitions for optimized compile times
