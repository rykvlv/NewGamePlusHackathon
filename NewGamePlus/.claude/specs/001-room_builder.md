I'm building an isometric room builder game in Unity 6.4 (URP).
Reference art style: pixel art isometric view, warm colors, clean outlines.
See the attached screenshot(room-builder-example.png) for the visual target — a room with isometric tiles 
on the floor and furniture items placed on them.

## Core systems to implement

### 1. Isometric grid
- Grid of tiles in isometric projection (classic 2:1 diamond ratio)
- Each tile can be one of two shapes:
  - Square tile (standard 1x1 diamond)
  - Diagonal/half tile (triangular, cuts the diamond diagonally) for wall edges and corners
- Tiles rendered with flat pixel-art style sprites
- Grid manager ScriptableObject that stores tile shape and state data

### 2. Tile editor mode
- In editor mode, click a tile to cycle its shape: square → diagonal-left → diagonal-right → empty
- Visual highlight on hover
- Tiles remember their shape at runtime (saved to ScriptableObject or JSON)

### 3. Item placement system
- Items (furniture) have a footprint: a list of local tile offsets they occupy
  (e.g. a 1x1 wardrobe occupies 1 tile, a 2x1 dresser occupies 2 tiles)
- When dragging an item, highlight the tiles it would occupy:
  - Green if all tiles are valid (correct shape, unoccupied)
  - Red if placement is blocked
- Confirm placement on click
- Items snap to grid in isometric space
- Items can be picked up and repositioned

### 4. Camera
- Fixed isometric camera angle (45° horizontal, ~30° vertical, matching the reference)
- No rotation — top-down isometric only
- Pan with middle mouse button or arrow keys

### 5. Architecture rules
- Use UniTask for any async work
- ScriptableObjects for item definitions (name, sprite, footprint, pivot offset)
- No FindObjectOfType at runtime
- Grid coordinates in 2D (col, row) converted to world space via an IsometricUtils static class
- Separate concerns: GridManager (data), GridRenderer (visuals), PlacementSystem (interaction)

## Deliver in this order
1. IsometricUtils.cs — coordinate conversion functions first, with unit tests
2. GridManager.cs + TileData.cs — grid data layer
3. GridRenderer.cs — renders tiles as sprites in isometric space  
4. PlacementSystem.cs — drag, highlight, confirm placement
5. ItemDefinition.asset (ScriptableObject) — one example wardrobe item
6. A simple scene wired up and ready to play

After each file: check Unity console via Unity_GetConsoleLogs and fix any errors before moving on.