using System.Collections.Generic;
using UnityEngine;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// ScriptableObject that owns the grid data layer.
    /// Stores all <see cref="TileData"/> and exposes query/mutation methods.
    /// </summary>
    [CreateAssetMenu(menuName = "Harmonize/GridManagerSO", fileName = "DefaultGrid")]
    public class GridManagerSO : ScriptableObject
    {
        /// <summary>Number of columns in the grid.</summary>
        [field: SerializeField] public int Columns { get; private set; } = 8;

        /// <summary>Number of rows in the grid.</summary>
        [field: SerializeField] public int Rows { get; private set; } = 8;

        /// <summary>Flat list of all tile data, indexed row-major.</summary>
        [SerializeField] private List<TileData> _tiles = new();

        private static readonly TileShape[] ShapeCycle =
        {
            TileShape.Square,
            TileShape.DiagonalLeft,
            TileShape.DiagonalRight,
            TileShape.Empty
        };

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
        /// Returns true when a tile exists at the given position, has a Square shape, and is not occupied.
        /// </summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public bool IsValidPlacement(int col, int row)
        {
            TileData tile = GetTile(col, row);
            return tile != null && tile.Shape == TileShape.Square && !tile.IsOccupied;
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
            tile.IsOccupied = false;
        }

        /// <summary>Sets the occupied flag on a tile.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        /// <param name="occupied">Desired occupied state.</param>
        public void SetOccupied(int col, int row, bool occupied)
        {
            TileData tile = GetTile(col, row);
            if (tile != null) tile.IsOccupied = occupied;
        }

        private bool InBounds(int col, int row) =>
            col >= 0 && col < Columns && row >= 0 && row < Rows;
    }
}
