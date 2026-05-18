using System.Collections.Generic;
using UnityEngine;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// Renders the isometric grid by spawning one SpriteRenderer per tile.
    /// Reads layout from a <see cref="GridManagerSO"/> and positions tiles using <see cref="IsometricUtils"/>.
    /// </summary>
    public class GridRenderer : MonoBehaviour
    {
        [SerializeField] private GridManagerSO _gridManager;
        [SerializeField] private Sprite _squareSprite;
        [SerializeField] private Sprite _diagonalLeftSprite;
        [SerializeField] private Sprite _diagonalRightSprite;

        [SerializeField] private float _cellWidth = 1f;
        [SerializeField] private float _cellHeight = 0.5f;

        private readonly Dictionary<(int, int), SpriteRenderer> _renderers = new();

        private void Start()
        {
            if (_gridManager == null)
            {
                Debug.LogError("[GridRenderer] GridManagerSO not assigned.", this);
                return;
            }

            if (_gridManager.GetTile(0, 0) == null)
                _gridManager.Initialize();

            BuildRenderers();
        }

        /// <summary>Refreshes the sprite and tint for a single tile.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public void RefreshTile(int col, int row)
        {
            if (_gridManager == null || !_renderers.TryGetValue((col, row), out SpriteRenderer sr)) return;
            TileData tile = _gridManager.GetTile(col, row);
            if (tile == null) return;

            sr.sprite = SpriteForShape(tile.Shape);
            sr.gameObject.SetActive(tile.Shape != TileShape.Empty);
            sr.color = Color.white;
        }

        /// <summary>Refreshes every tile in the grid.</summary>
        public void RefreshAll()
        {
            if (_gridManager == null) return;
            foreach (var kv in _renderers)
                RefreshTile(kv.Key.Item1, kv.Key.Item2);
        }

        /// <summary>Applies a highlight colour to a specific tile's SpriteRenderer.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        /// <param name="color">Tint colour to apply.</param>
        public void SetTileHighlight(int col, int row, Color color)
        {
            if (_renderers.TryGetValue((col, row), out SpriteRenderer sr))
                sr.color = color;
        }

        /// <summary>Resets all tiles to white (no highlight).</summary>
        public void ClearHighlights()
        {
            foreach (var sr in _renderers.Values)
                sr.color = Color.white;
        }

        /// <summary>Returns the world-space position for the given grid coordinate.</summary>
        /// <param name="col">Column index.</param>
        /// <param name="row">Row index.</param>
        public Vector3 GetWorldPosition(int col, int row) =>
            IsometricUtils.GridToWorld(col, row, _cellWidth, _cellHeight);

        /// <summary>Converts a world position to the nearest grid coordinate.</summary>
        /// <param name="worldPos">Position in world space.</param>
        public Vector2Int WorldToGrid(Vector3 worldPos) =>
            IsometricUtils.WorldToGrid(worldPos, _cellWidth, _cellHeight);

        private void BuildRenderers()
        {
            foreach (Transform child in transform)
                Destroy(child.gameObject);
            _renderers.Clear();

            for (int r = 0; r < _gridManager.Rows; r++)
            {
                for (int c = 0; c < _gridManager.Columns; c++)
                {
                    TileData tile = _gridManager.GetTile(c, r);
                    if (tile == null) continue;

                    GameObject go = new GameObject($"Tile_{c}_{r}");
                    go.transform.SetParent(transform, false);
                    go.transform.localPosition = IsometricUtils.GridToWorld(c, r, _cellWidth, _cellHeight);

                    SpriteRenderer sr = go.AddComponent<SpriteRenderer>();
                    sr.sprite = SpriteForShape(tile.Shape);
                    sr.color = Color.white;

                    // Sort by row desc so "lower" rows appear in front of higher rows.
                    sr.sortingOrder = -r;

                    go.SetActive(tile.Shape != TileShape.Empty);
                    _renderers[(c, r)] = sr;
                }
            }
        }

        private Sprite SpriteForShape(TileShape shape)
        {
            return shape switch
            {
                TileShape.Square => _squareSprite,
                TileShape.DiagonalLeft => _diagonalLeftSprite,
                TileShape.DiagonalRight => _diagonalRightSprite,
                _ => null
            };
        }
    }
}
