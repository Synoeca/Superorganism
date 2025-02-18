# Superorganism v0.2.0
![image](https://github.com/user-attachments/assets/d8296160-dbe5-4664-9a51-b4a618fb6b38)

## Save/Load System Enhancement:
- **Advanced map state persistence** with isolated map copies per save
- **Dynamic .tmx modification** with runtime support
- **Enhanced save file management** with proper cleanup
---


https://github.com/user-attachments/assets/dc805279-be1f-48f5-b704-57c9259cc0b8



## Core Game Features:
- **Interactive Ground Digging (Press 'F')**:
  - Press 'F' while moving left/right to dig in that direction
  - Press 'F' + Up/W to dig upward from current position
  - Press 'F' while stationary to dig directly below
  - Smart position adjustment based on entity size and direction
- **Sophisticated collision resolution** for controlled entities
- **Enhanced map modification** with per-tile opacity support
---

## Technical Implementation Details:
- Complete overhaul of save/load system with unique map states
- Runtime .tmx modification and tile property management
- Advanced collision handling for diagonal and flat surfaces
- Context-aware digging system with:
  - Direction-based offset calculation
  - Entity size consideration
  - Multi-directional capability
- Comprehensive save file cleanup with associated map files
---
![image](https://github.com/user-attachments/assets/f3bf73c9-079e-44b3-a00c-42d949fa3ef2)

## Game Objective
- **Collect all crops** scattered throughout the map to win
- **Avoid hostile entities**:
  - Red ants patrolling with smart AI
  - Flying enemies with unique movement patterns
- **Use digging mechanics** to create pathways and escape routes
---
## Fixed:
- Multiple collision-related issues:
  - Entity sinking during enemy chase
  - Diagonal tile climbing during pursuit
  - Ground sinking from proposed Y movement
  - Center-on-diagonal collision detection
  - Jump landing on diagonal/flat tile borders
  - Negative slope tile collision
  - Right diagonal collision issues
  - Upward collision detection
- Save system issues:
  - Continue button persistence after save removal
  - Save file cleanup and management
  - Map file association tracking

## Changed:
- Revamped TilemapEngine without Content Pipeline dependency
- Modified save file naming scheme
- Updated collision handling system
- Restructured map file management
- Enhanced tile property system
- Improved diagonal collision response

## Added:
- Ground digging feature
- Per-tile opacity support
- Runtime .tmx modification
- Map state isolation per save
- Comprehensive save file cleanup
- Upward digging capability
- Enhanced collision documentation

## Removed:
- Content Pipeline dependency
- Redundant tile properties
- Unnecessary position UI
- Legacy collision systems
- Unused comments

## Known Issues:
- While collision system for ControllableEntity is mostly resolved, uncontrolled entities (AI) still need to implement the same sophisticated collision logic

## Future Development Plans:
- Implementation of core RPG elements:
  - Stamina system
  - Hunger mechanics
  - Inventory system
  - Collectables
  - Character stats
---
