# Superorganism v0.3.0 (Version 1.0)
![Game Title Screen](https://github.com/user-attachments/assets/ce9200bb-805c-4002-9363-8c78df979b27)


Welcome to Superorganism, a platformer game where you navigate through complex ecosystems, collect resources, and survive hostile environments by mastering digging, inventory management, and combat mechanics.

## Key Features

### Enhanced Movement & Combat System
- **Robust Collision Detection**:
  - Complete solution for diagonal and flat surface handling
  - Intelligent collision resolution for various surface types
  - Fixed numerous edge cases including diagonal climbing and sinking issues
- **Jumping Mechanics**:
  - Cooldown timer between jumps for balanced gameplay
  - Fixed flipped jump collision detection
  - Improved upward and diagonal jumping physics
- **Combat Capabilities**:
  - Enemy stomping mechanic
  - Stamina-based attacks and movement
  - Health system with hit points (HP)

![image](https://github.com/user-attachments/assets/eb0a649b-81de-418e-8ddd-606d325a42a6)

### Advanced Inventory System
- **Dynamic Item Management**:
  - Resizable inventory UI with minimize/maximize controls
  - Tileset-based inventory item textures
  - Seamless item pickup and usage
- **Item Interaction**:
  - Throw and collect items
  - Nearest item indicator for improved gameplay experience
  - Visual cues for available collectibles

### Resource Management
- **Survival Mechanics**:
  - Stamina system with gradual regeneration
  - Hunger system affecting overall performance
  - Float-based resource values for precise management
- **UI Indicators**:
  - Real-time status displays for stamina, hunger, and HP
  - Visual feedback for resource consumption and regeneration

### World Interaction
- **Interactive Ground Digging** (Press 'F'):
  - Multi-directional digging based on movement and input
  - Strategic pathway creation for navigation
  - Escape route planning from enemies
- **Tilemap Modification**:
  - Runtime .tmx file editing
  - Per-tile opacity support
  - Dynamic environment changes

### Game Progress Tracking
- **In-Game Timer**:
  - Track your gameplay duration
  - Timer persistence between save/load cycles
- **Save/Load System**:
  - Advanced map state persistence with isolated map copies per save
  - Comprehensive save file management with proper cleanup
  - Fixed issues with collectible items and modified tilemaps

## Game Objective
- **Collect all resources** scattered throughout the map
- **Manage your survival resources** (stamina, hunger, health)
- **Avoid or combat hostile entities**:
  - Red ants with intelligent patrol AI
  - Flying enemies with unique movement patterns
- **Create strategic pathways** using the digging mechanic

## Controls
- **Movement**: WASD or Arrow Keys
- **Jump**: Space
- **Dig**: F + Direction (F alone digs downward)
- **Throw Item**: X
- **Inventory**: I

## Technical Improvements
- Complete overhaul of the save/load system with unique map states
- Removed Content Pipeline dependency for improved portability
- Enhanced AI strategy behaviors with save state persistence
- Revamped TilemapEngine for better performance
- Advanced collision handling with precise physics calculations
- Seamless menu transitions with parent-child screen relationships

## Fixed Issues Since v0.2.0
- Complete resolution of diagonal collision problems
- Save system reliability with proper file management
- AI strategy persistence across save/load cycles
- Modified tilemap and collectible item loading
- UI scaling and positioning
- Entity status synchronization with UI elements

## Known Issues
- While AI entities have improved, some edge cases in their collision detection may still occur

## Future Development
- Enhanced RPG elements:
  - Expanded inventory system with crafting
  - More detailed character progression
  - Advanced enemy AI patterns
  - Environmental interactions
  - Quest system

---
