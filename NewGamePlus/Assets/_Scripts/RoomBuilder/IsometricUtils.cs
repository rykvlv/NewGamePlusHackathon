using UnityEngine;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// Static utility for converting between 2D grid coordinates and isometric world space.
    /// Uses the classic 2:1 diamond projection.
    /// </summary>
    public static class IsometricUtils
    {
        /// <summary>
        /// Converts grid column/row to an isometric world position.
        /// </summary>
        /// <param name="col">Grid column index.</param>
        /// <param name="row">Grid row index.</param>
        /// <param name="cellWidth">World-space width of one cell. Default is 1.</param>
        /// <param name="cellHeight">World-space height of one cell. Default is 0.5.</param>
        /// <returns>World position for the tile centre.</returns>
        public static Vector3 GridToWorld(int col, int row, float cellWidth = 1f, float cellHeight = 0.5f)
        {
            float x = (col - row) * cellWidth * 0.5f;
            float y = (col + row) * cellHeight * 0.5f;
            return new Vector3(x, y, 0f);
        }

        /// <summary>
        /// Converts an isometric world position back to the nearest grid coordinate.
        /// </summary>
        /// <param name="worldPos">Position in world space.</param>
        /// <param name="cellWidth">World-space width of one cell. Default is 1.</param>
        /// <param name="cellHeight">World-space height of one cell. Default is 0.5.</param>
        /// <returns>The grid column and row closest to the given world position.</returns>
        public static Vector2Int WorldToGrid(Vector3 worldPos, float cellWidth = 1f, float cellHeight = 0.5f)
        {
            // Inverse of GridToWorld:
            // x = (col - row) * cellWidth * 0.5  => col - row = x * 2 / cellWidth
            // y = (col + row) * cellHeight * 0.5  => col + row = y * 2 / cellHeight
            float colPlusRow = worldPos.y * 2f / cellHeight;
            float colMinusRow = worldPos.x * 2f / cellWidth;
            int col = Mathf.RoundToInt((colPlusRow + colMinusRow) * 0.5f);
            int row = Mathf.RoundToInt((colPlusRow - colMinusRow) * 0.5f);
            return new Vector2Int(col, row);
        }
    }
}
