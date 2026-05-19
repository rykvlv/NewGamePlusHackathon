using UnityEditor;
using UnityEngine;

namespace Harmonize.RoomBuilder.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="GridManagerSO"/> that provides:
    /// <list type="bullet">
    /// <item>A resize section for changing grid dimensions.</item>
    /// <item>A property drawer for initial item placements.</item>
    /// <item>An interactive scrollable tile grid for cycling <see cref="TileShape"/> values.</item>
    /// </list>
    /// </summary>
    [CustomEditor(typeof(GridManagerSO))]
    public sealed class GridManagerSOInspector : UnityEditor.Editor
    {
        private const float CellSize = 28f;
        private const float CellSpacing = 2f;
        private const float ScrollViewMaxHeight = 500f;
        private const int MinDimension = 1;
        private const int MaxDimension = 100;

        private static readonly Color SquareColor       = new Color(0.53f, 0.81f, 0.98f, 1f); // light blue
        private static readonly Color DiagLeftColor     = new Color(0.98f, 0.96f, 0.53f, 1f); // light yellow
        private static readonly Color DiagRightColor    = new Color(0.98f, 0.75f, 0.42f, 1f); // light orange
        private static readonly Color EmptyColor        = new Color(0.22f, 0.22f, 0.22f, 1f); // dark gray

        private int _pendingColumns;
        private int _pendingRows;
        private Vector2 _gridScrollPos;
        private bool _dimensionsInitialised;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            GridManagerSO gridManager = (GridManagerSO)target;

            if (!_dimensionsInitialised)
            {
                _pendingColumns = gridManager.Columns;
                _pendingRows    = gridManager.Rows;
                _dimensionsInitialised = true;
            }

            DrawDimensionInfo(gridManager);
            EditorGUILayout.Space(8f);

            DrawInitialItemsList();
            EditorGUILayout.Space(8f);

            DrawTileGrid(gridManager);
            EditorGUILayout.Space(4f);

            DrawReinitializeButton(gridManager);

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawDimensionInfo(GridManagerSO gridManager)
        {
            EditorGUILayout.LabelField("Grid Dimensions", EditorStyles.boldLabel);

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField($"Current: {gridManager.Columns} x {gridManager.Rows}", GUILayout.Width(140f));
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.Space(4f);
            EditorGUILayout.LabelField("Resize Grid", EditorStyles.miniBoldLabel);
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Columns", GUILayout.Width(55f));
            _pendingColumns = Mathf.Clamp(EditorGUILayout.IntField(_pendingColumns, GUILayout.Width(45f)), MinDimension, MaxDimension);
            EditorGUILayout.LabelField("Rows", GUILayout.Width(35f));
            _pendingRows = Mathf.Clamp(EditorGUILayout.IntField(_pendingRows, GUILayout.Width(45f)), MinDimension, MaxDimension);

            if (GUILayout.Button("Apply", GUILayout.Width(55f)))
            {
                ApplyResize(gridManager);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ApplyResize(GridManagerSO gridManager)
        {
            Undo.RecordObject(target, "Resize Grid");

            SerializedProperty colProp = serializedObject.FindProperty("_columns");
            SerializedProperty rowProp = serializedObject.FindProperty("_rows");

            if (colProp != null) colProp.intValue = _pendingColumns;
            if (rowProp != null) rowProp.intValue = _pendingRows;
            serializedObject.ApplyModifiedProperties();

            gridManager.Initialize();
            EditorUtility.SetDirty(target);
        }

        private void DrawInitialItemsList()
        {
            EditorGUILayout.LabelField("Initial Item Placements", EditorStyles.boldLabel);
            SerializedProperty initialItemsProp = serializedObject.FindProperty("_initialItems");
            if (initialItemsProp != null)
                EditorGUILayout.PropertyField(initialItemsProp, includeChildren: true);
        }

        private void DrawTileGrid(GridManagerSO gridManager)
        {
            EditorGUILayout.LabelField("Tile Shape Editor", EditorStyles.boldLabel);
            EditorGUILayout.HelpBox(
                "Click a cell to cycle its shape: Square (S) → DiagLeft (L) → DiagRight (R) → Empty (·) → Square",
                MessageType.None);

            int cols = gridManager.Columns;
            int rows = gridManager.Rows;

            float totalWidth  = cols * (CellSize + CellSpacing);
            float totalHeight = rows * (CellSize + CellSpacing);

            _gridScrollPos = EditorGUILayout.BeginScrollView(
                _gridScrollPos,
                GUILayout.Height(Mathf.Min(totalHeight + 8f, ScrollViewMaxHeight))
            );

            // Draw rows top-to-bottom: row 0 at the bottom, highest row at the top.
            for (int row = rows - 1; row >= 0; row--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < cols; col++)
                {
                    TileData tile = gridManager.GetTile(col, row);
                    if (tile == null)
                    {
                        GUILayout.Space(CellSize + CellSpacing);
                        continue;
                    }

                    Color cellColor = ShapeToColor(tile.Shape);
                    string label   = ShapeToLabel(tile.Shape);

                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = cellColor;

                    if (GUILayout.Button(label, GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                    {
                        Undo.RecordObject(target, "Cycle Tile Shape");
                        gridManager.CycleTileShape(col, row);
                        EditorUtility.SetDirty(target);
                    }

                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void DrawReinitializeButton(GridManagerSO gridManager)
        {
            if (GUILayout.Button("Reinitialize Grid"))
            {
                Undo.RecordObject(target, "Reinitialize Grid");
                gridManager.Initialize();
                EditorUtility.SetDirty(target);
            }
        }

        private static Color ShapeToColor(TileShape shape) => shape switch
        {
            TileShape.Square        => SquareColor,
            TileShape.DiagonalLeft  => DiagLeftColor,
            TileShape.DiagonalRight => DiagRightColor,
            _                       => EmptyColor
        };

        private static string ShapeToLabel(TileShape shape) => shape switch
        {
            TileShape.Square        => "S",
            TileShape.DiagonalLeft  => "L",
            TileShape.DiagonalRight => "R",
            _                       => "·"
        };
    }
}
