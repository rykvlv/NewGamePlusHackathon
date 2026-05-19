using System.Collections.Generic;
using UnityEngine;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// Describes an item that should be pre-placed on the grid when the scene starts.
    /// </summary>
    [System.Serializable]
    public class InitialItemPlacement
    {
        /// <summary>The item definition to place.</summary>
        public ItemDefinition Item;

        /// <summary>Anchor grid position (column, row) for the placement.</summary>
        public Vector2Int Position;

        /// <summary>Clockwise rotation in degrees. Must be one of 0, 90, 180, or 270.</summary>
        public int Rotation;
    }

    /// <summary>
    /// ScriptableObject that owns the grid data layer.
    /// Stores all <see cref="TileData"/> and exposes query/mutation methods.
    /// Occupied state is tracked at runtime only and is never serialised.
    /// </summary>
    [CreateAssetMenu(menuName = "Harmonize/GridManagerSO", fileName = "DefaultGrid")]
    public class GridManagerSO : ScriptableObject
    {
        /// <summary>Number of columns in the grid.</summary>
        [SerializeField] private int _columns = 8;
        public int Columns => _columns;

        /// <summary>Number of rows in the grid.</summary>
        [SerializeField] private int _rows = 8;
        public int Rows => _rows;

        /// <summary>Flat list of all tile data, indexed row-major.</summary>
        [SerializeField] private List<TileData> _tiles = new();

        /// <summary>Items to spawn automatically when the scene starts.</summary>
        [SerializeField] private List<InitialItemPlacement> _initialItems = new();

        /// <summary>Read-only view of the initial item placements list.</summary>
        public IReadOnlyList<InitialItemPlacement> InitialItems => _initialItems;

        // Runtime-only occupancy — not serialised so it resets on play mode entry.
        private HashSet<(int col, int row)> _occupiedAtRuntime;

        private static readonly TileShape[] ShapeCycle =
        {
            TileShape.Square,
            TileShape.DiagonalLeft,
            TileShape.DiagonalRight,
            TileShape.Empty
        };

        private void OnEnable() => _occupiedAtRuntime = new HashSet<(int, int)>();

        private void OnDisable() => _occupiedAtRuntime = new HashSet<(int, int)>();

        /// <summary>
        /// Populates the tile list with default Square tiles.
        /// Call this once from the editor or on first use.
        /// </summary>
        public void Initialize()
        {
            _tiles.Clear();
            for (int r = 0; r < Rows; r++)
            {
                for (int c = 0; c < Columns; c++)
                {
                    _tiles.Add(new TileData { Col = c, Row = r, Shape = TileShape.Square });
                }
            }
        }

        /// <summary>Returns the <see cref="TileData"/> for the given coordinates, or null if out of range.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public TileData GetTile(int col, int row)
        {
            if (!InBounds(col, row)) return null;
            return _tiles[row * Columns + col];
        }

        /// <summary>
        /// Returns true when a tile exists at the given position, has a Square shape, and is not occupied at runtime.
        /// </summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public bool IsValidPlacement(int col, int row)
        {
            TileData tile = GetTile(col, row);
            if (tile == null || tile.Shape != TileShape.Square) return false;
            return !IsOccupied(col, row);
        }

        /// <summary>Returns true if the given tile is currently marked as occupied at runtime.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public bool IsOccupied(int col, int row)
        {
            return _occupiedAtRuntime != null && _occupiedAtRuntime.Contains((col, row));
        }

        /// <summary>
        /// Advances the tile's shape through the cycle: Square → DiagonalLeft → DiagonalRight → Empty → Square.
        /// </summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public void CycleTileShape(int col, int row)
        {
            TileData tile = GetTile(col, row);
            if (tile == null) return;

            int currentIndex = System.Array.IndexOf(ShapeCycle, tile.Shape);
            tile.Shape = ShapeCycle[(currentIndex + 1) % ShapeCycle.Length];
        }

        /// <summary>Sets the runtime occupied state for a tile.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        /// <param name="occupied">Desired occupied state.</param>
        public void SetOccupied(int col, int row, bool occupied)
        {
            if (_occupiedAtRuntime == null) _occupiedAtRuntime = new HashSet<(int, int)>();

            if (occupied)
                _occupiedAtRuntime.Add((col, row));
            else
                _occupiedAtRuntime.Remove((col, row));
        }

        private bool InBounds(int col, int row) =>
            col >= 0 && col < Columns && row >= 0 && row < Rows;
    }
}
