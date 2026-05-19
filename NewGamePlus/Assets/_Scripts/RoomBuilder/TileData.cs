using System;
using UnityEngine;

namespace Harmonize.RoomBuilder
{
    /// <summary>Describes the visual/logical shape of a single grid tile.</summary>
    public enum TileShape
    {
        Empty,
        Square,
        DiagonalLeft,
        DiagonalRight
    }

    /// <summary>
    /// Data container for one cell in the isometric grid.
    /// Serialized directly inside <see cref="GridManagerSO"/>.
    /// </summary>
    [Serializable]
    public class TileData
    {
        /// <summary>Column index in the grid.</summary>
        public int Col;

        /// <summary>Row index in the grid.</summary>
        public int Row;

        /// <summary>Shape variant rendered for this tile.</summary>
        public TileShape Shape;
    }
}
