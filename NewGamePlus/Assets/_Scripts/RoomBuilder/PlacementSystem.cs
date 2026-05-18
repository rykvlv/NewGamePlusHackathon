using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// Handles two interaction modes for the isometric grid:
    /// <list type="bullet">
    /// <item><term>TileEdit</term><description>Hover-highlight and click to cycle tile shapes.</description></item>
    /// <item><term>ItemPlace</term><description>Drag an item ghost, show green/red footprint, confirm or cancel placement.</description></item>
    /// </list>
    /// </summary>
    public class PlacementSystem : MonoBehaviour
    {
        private const float GhostAlpha = 0.5f;
        private const int GhostSortingOrder = 100;
        private const int PlacedItemSortingOrder = 50;

        /// <summary>Interaction mode enumeration.</summary>
        public enum Mode { TileEdit, ItemPlace }

        [Header("References")]
        [SerializeField] private GridManagerSO _gridManager;
        [SerializeField] private GridRenderer _gridRenderer;
        [SerializeField] private Camera _camera;

        [Header("Test")]
        [SerializeField] private ItemDefinition _testItem;

        [Header("Highlight colours")]
        [SerializeField] private Color _hoverColor = Color.yellow;
        [SerializeField] private Color _validColor = new Color(0f, 1f, 0f, 0.6f);
        [SerializeField] private Color _invalidColor = new Color(1f, 0f, 0f, 0.6f);

        [Header("Events")]
        /// <summary>Fires when an item is successfully placed.</summary>
        public UnityEvent<ItemDefinition, Vector3> OnItemPlaced;
        /// <summary>Fires when an item placement is cancelled.</summary>
        public UnityEvent OnPlacementCancelled;

        private Mode _currentMode = Mode.TileEdit;
        private ItemDefinition _draggedItem;
        private GameObject _ghostObject;
        private SpriteRenderer _ghostRenderer;

        // Tracks placed item sprites so they can be clicked to pick up again.
        private readonly List<PlacedItem> _placedItems = new();

        private Vector2Int _lastHoveredTile = new Vector2Int(-1, -1);

        private struct PlacedItem
        {
            public ItemDefinition Definition;
            public Vector2Int[] OccupiedTiles;
            public GameObject Visual;
        }

        private bool _refsValid;

        private void Awake()
        {
            _refsValid = true;
            if (_gridManager == null)  { Debug.LogError("[PlacementSystem] GridManagerSO not assigned.", this); _refsValid = false; }
            if (_gridRenderer == null) { Debug.LogError("[PlacementSystem] GridRenderer not assigned.", this);  _refsValid = false; }
            if (_camera == null)       { Debug.LogError("[PlacementSystem] Camera not assigned.", this);        _refsValid = false; }
        }

        private void Update()
        {
            if (!_refsValid) return;

            // Press P to start placing the test item, Escape to cancel.
            if (_testItem != null && Keyboard.current.pKey.wasPressedThisFrame)
                BeginDrag(_testItem);
            if (Keyboard.current.escapeKey.wasPressedThisFrame)
                CancelDrag();

            Vector2Int hoveredTile = GetHoveredTile();

            if (_currentMode == Mode.TileEdit)
                UpdateTileEditMode(hoveredTile);
            else
                UpdateItemPlaceMode(hoveredTile);

            _lastHoveredTile = hoveredTile;
        }

        // ── Public API ────────────────────────────────────────────────────────────

        /// <summary>Switches the current interaction mode.</summary>
        /// <param name="mode">The mode to activate.</param>
        public void SetMode(Mode mode)
        {
            CancelDrag();
            _currentMode = mode;
        }

        /// <summary>
        /// Begins an item-placement drag with the given definition.
        /// Automatically switches to ItemPlace mode.
        /// </summary>
        /// <param name="item">Item to place.</param>
        public void BeginDrag(ItemDefinition item)
        {
            if (item == null) return;
            _currentMode = Mode.ItemPlace;
            _draggedItem = item;

            _ghostObject = new GameObject("Ghost");
            _ghostRenderer = _ghostObject.AddComponent<SpriteRenderer>();
            _ghostRenderer.sprite = item.Icon;
            _ghostRenderer.color = new Color(1f, 1f, 1f, GhostAlpha);
            _ghostRenderer.sortingOrder = GhostSortingOrder;
        }

        // ── Private update helpers ────────────────────────────────────────────────

        private void UpdateTileEditMode(Vector2Int hovered)
        {
            if (hovered != _lastHoveredTile)
            {
                _gridRenderer.ClearHighlights();
                if (_gridManager.GetTile(hovered.x, hovered.y) != null)
                    _gridRenderer.SetTileHighlight(hovered.x, hovered.y, _hoverColor);
            }

            if (!Mouse.current.leftButton.wasPressedThisFrame) return;

            // Try to pick up a previously placed item first.
            if (TryPickUpItem(hovered)) return;

            // Otherwise cycle the tile shape under the cursor.
            TileData tile = _gridManager.GetTile(hovered.x, hovered.y);
            if (tile != null)
            {
                _gridManager.CycleTileShape(hovered.x, hovered.y);
                _gridRenderer.RefreshTile(hovered.x, hovered.y);
                _gridRenderer.SetTileHighlight(hovered.x, hovered.y, _hoverColor);
            }
        }

        private void UpdateItemPlaceMode(Vector2Int hovered)
        {
            if (_draggedItem == null) return;

            Vector2Int[] footprintTiles = ComputeFootprint(hovered);
            bool allValid = AllTilesValid(footprintTiles);

            _gridRenderer.ClearHighlights();
            Color highlight = allValid ? _validColor : _invalidColor;
            foreach (Vector2Int t in footprintTiles)
                _gridRenderer.SetTileHighlight(t.x, t.y, highlight);

            // Position ghost at footprint centroid
            Vector3 centroid = ComputeCentroid(footprintTiles);
            if (_ghostObject != null)
                _ghostObject.transform.position = centroid + (Vector3)_draggedItem.PivotOffset;

            if (Mouse.current.leftButton.wasPressedThisFrame && allValid)
                ConfirmPlacement(hovered, footprintTiles, centroid);

            if (Mouse.current.rightButton.wasPressedThisFrame)
                CancelDrag();
        }

        private bool TryPickUpItem(Vector2Int tile)
        {
            for (int i = _placedItems.Count - 1; i >= 0; i--)
            {
                PlacedItem item = _placedItems[i];
                foreach (Vector2Int t in item.OccupiedTiles)
                {
                    if (t != tile) continue;

                    foreach (Vector2Int ot in item.OccupiedTiles)
                        _gridManager.SetOccupied(ot.x, ot.y, false);

                    _gridRenderer.RefreshAll();
                    Destroy(item.Visual);
                    _placedItems.RemoveAt(i);

                    BeginDrag(item.Definition);
                    return true;
                }
            }
            return false;
        }

        private void ConfirmPlacement(Vector2Int anchor, Vector2Int[] footprintTiles, Vector3 centroid)
        {
            foreach (Vector2Int t in footprintTiles)
            {
                _gridManager.SetOccupied(t.x, t.y, true);
                _gridRenderer.RefreshTile(t.x, t.y);
            }

            GameObject visual = new GameObject($"Item_{_draggedItem.ItemName}");
            SpriteRenderer sr = visual.AddComponent<SpriteRenderer>();
            sr.sprite = _draggedItem.Icon;
            sr.sortingOrder = PlacedItemSortingOrder;
            visual.transform.position = centroid + (Vector3)_draggedItem.PivotOffset;

            _placedItems.Add(new PlacedItem
            {
                Definition = _draggedItem,
                OccupiedTiles = footprintTiles,
                Visual = visual
            });

            OnItemPlaced?.Invoke(_draggedItem, centroid);

            CleanUpGhost();
            _draggedItem = null;
            _currentMode = Mode.TileEdit;
            _gridRenderer.ClearHighlights();
        }

        private void CancelDrag()
        {
            if (_draggedItem != null)
                OnPlacementCancelled?.Invoke();

            CleanUpGhost();
            _draggedItem = null;
            _currentMode = Mode.TileEdit;
            _gridRenderer.ClearHighlights();
        }

        private void CleanUpGhost()
        {
            if (_ghostObject != null)
            {
                Destroy(_ghostObject);
                _ghostObject = null;
                _ghostRenderer = null;
            }
        }

        // ── Geometry helpers ──────────────────────────────────────────────────────

        private Vector2Int GetHoveredTile()
        {
            Vector3 mouseScreen = Mouse.current.position.ReadValue();
            // For an orthographic camera the z value fed to ScreenToWorldPoint is the
            // distance from the camera plane, not the world z; use 0 so we hit z=0.
            mouseScreen.z = _camera.nearClipPlane;
            Vector3 worldPos = _camera.ScreenToWorldPoint(mouseScreen);
            worldPos.z = 0f;
            return _gridRenderer.WorldToGrid(worldPos);
        }

        private Vector2Int[] ComputeFootprint(Vector2Int anchor)
        {
            Vector2Int[] offsets = _draggedItem.Footprint;
            if (offsets == null || offsets.Length == 0)
                return new[] { anchor };

            Vector2Int[] result = new Vector2Int[offsets.Length];
            for (int i = 0; i < offsets.Length; i++)
                result[i] = anchor + offsets[i];
            return result;
        }

        private bool AllTilesValid(Vector2Int[] tiles)
        {
            foreach (Vector2Int t in tiles)
                if (!_gridManager.IsValidPlacement(t.x, t.y)) return false;
            return true;
        }

        private Vector3 ComputeCentroid(Vector2Int[] tiles)
        {
            Vector3 sum = Vector3.zero;
            foreach (Vector2Int t in tiles)
                sum += _gridRenderer.GetWorldPosition(t.x, t.y);
            return sum / tiles.Length;
        }
    }
}
