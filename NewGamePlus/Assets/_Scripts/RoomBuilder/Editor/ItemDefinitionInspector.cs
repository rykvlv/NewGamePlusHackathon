using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Harmonize.RoomBuilder.Editor
{
    /// <summary>
    /// Custom inspector for <see cref="ItemDefinition"/> that draws an interactive
    /// footprint grid editor. Cells can be toggled on/off to define which tiles the
    /// item occupies relative to its anchor at (0,0).
    /// </summary>
    [CustomEditor(typeof(ItemDefinition))]
    public sealed class ItemDefinitionInspector : UnityEditor.Editor
    {
        private const int MinCanvasSize = 1;
        private const int MaxCanvasWidth = 50;
        private const int MaxCanvasHeight = 80;
        private const int DefaultCanvasSize = 10;
        private const float CellSize = 22f;
        private const float CellSpacing = 2f;
        private const float ScrollViewMaxHeight = 400f;

        private static readonly Color ActiveCellColor  = new Color(0.2f, 0.8f, 0.2f, 1f);
        private static readonly Color InactiveCellColor = new Color(0.35f, 0.35f, 0.35f, 1f);
        private static readonly Color AnchorCellColor  = new Color(0.2f, 0.5f, 1f, 1f);

        private int _canvasW = DefaultCanvasSize;
        private int _canvasH = DefaultCanvasSize;
        private Vector2 _scrollPos;

        // Pending resize inputs.
        private int _pendingW = DefaultCanvasSize;
        private int _pendingH = DefaultCanvasSize;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            // Standard fields.
            EditorGUILayout.PropertyField(serializedObject.FindProperty("ItemName"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("Icon"));
            EditorGUILayout.PropertyField(serializedObject.FindProperty("PivotOffset"));

            EditorGUILayout.Space(8f);
            EditorGUILayout.LabelField("Footprint Editor", EditorStyles.boldLabel);

            DrawCanvasResizeControls();

            EditorGUILayout.Space(4f);

            DrawFootprintGrid();

            EditorGUILayout.Space(4f);
            DrawFooterControls();

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawCanvasResizeControls()
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Canvas Size", GUILayout.Width(80f));
            _pendingW = Mathf.Clamp(EditorGUILayout.IntField(_pendingW, GUILayout.Width(45f)), MinCanvasSize, MaxCanvasWidth);
            EditorGUILayout.LabelField("x", GUILayout.Width(12f));
            _pendingH = Mathf.Clamp(EditorGUILayout.IntField(_pendingH, GUILayout.Width(45f)), MinCanvasSize, MaxCanvasHeight);

            if (GUILayout.Button("Resize Canvas", GUILayout.Width(110f)))
            {
                ResizeCanvas(_pendingW, _pendingH);
            }
            EditorGUILayout.EndHorizontal();
        }

        private void ResizeCanvas(int newW, int newH)
        {
            _canvasW = newW;
            _canvasH = newH;

            ItemDefinition item = (ItemDefinition)target;
            Undo.RecordObject(item, "Resize Footprint Canvas");

            if (item.Footprint == null)
            {
                item.Footprint = new Vector2Int[0];
                EditorUtility.SetDirty(target);
                return;
            }

            List<Vector2Int> kept = new List<Vector2Int>(item.Footprint.Length);
            foreach (Vector2Int cell in item.Footprint)
            {
                if (cell.x >= 0 && cell.x < newW && cell.y >= 0 && cell.y < newH)
                    kept.Add(cell);
            }

            item.Footprint = kept.ToArray();
            EditorUtility.SetDirty(target);
        }

        private void DrawFootprintGrid()
        {
            ItemDefinition item = (ItemDefinition)target;
            HashSet<Vector2Int> activeSet = BuildActiveSet(item.Footprint);

            float totalWidth  = _canvasW * (CellSize + CellSpacing);
            float totalHeight = _canvasH * (CellSize + CellSpacing);

            _scrollPos = EditorGUILayout.BeginScrollView(
                _scrollPos,
                GUILayout.Height(Mathf.Min(totalHeight + 8f, ScrollViewMaxHeight))
            );

            // Rows are drawn top-to-bottom but y=0 is at the bottom conceptually, so
            // we iterate rows in descending order so the anchor cell (0,0) is at the bottom-left.
            for (int row = _canvasH - 1; row >= 0; row--)
            {
                EditorGUILayout.BeginHorizontal();
                for (int col = 0; col < _canvasW; col++)
                {
                    Vector2Int cell = new Vector2Int(col, row);
                    bool isActive = activeSet.Contains(cell);
                    bool isAnchor = col == 0 && row == 0;

                    Color buttonColor = isAnchor ? AnchorCellColor
                                      : isActive ? ActiveCellColor
                                      : InactiveCellColor;

                    Color prev = GUI.backgroundColor;
                    GUI.backgroundColor = buttonColor;

                    string label = isAnchor ? "A" : (isActive ? "X" : ".");
                    if (GUILayout.Button(label, GUILayout.Width(CellSize), GUILayout.Height(CellSize)))
                    {
                        ToggleCell(item, cell, activeSet);
                    }

                    GUI.backgroundColor = prev;
                }
                EditorGUILayout.EndHorizontal();
            }

            EditorGUILayout.EndScrollView();
        }

        private void ToggleCell(ItemDefinition item, Vector2Int cell, HashSet<Vector2Int> activeSet)
        {
            Undo.RecordObject(item, "Toggle Footprint Cell");

            if (activeSet.Contains(cell))
                activeSet.Remove(cell);
            else
                activeSet.Add(cell);

            item.Footprint = new Vector2Int[activeSet.Count];
            activeSet.CopyTo(item.Footprint);
            EditorUtility.SetDirty(target);
        }

        private void DrawFooterControls()
        {
            ItemDefinition item = (ItemDefinition)target;
            int count = item.Footprint != null ? item.Footprint.Length : 0;

            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Clear All", GUILayout.Width(80f)))
            {
                Undo.RecordObject(item, "Clear Footprint");
                item.Footprint = new Vector2Int[0];
                EditorUtility.SetDirty(target);
            }
            EditorGUILayout.LabelField($"Active cells: {count}", EditorStyles.miniLabel);
            EditorGUILayout.EndHorizontal();
        }

        private static HashSet<Vector2Int> BuildActiveSet(Vector2Int[] footprint)
        {
            HashSet<Vector2Int> set = new HashSet<Vector2Int>();
            if (footprint == null) return set;
            foreach (Vector2Int v in footprint)
                set.Add(v);
            return set;
        }
    }
}
