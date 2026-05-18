using UnityEngine;

namespace Harmonize.RoomBuilder
{
    /// <summary>
    /// ScriptableObject that defines a placeable room item — its icon, footprint, and display pivot.
    /// </summary>
    [CreateAssetMenu(menuName = "Harmonize/ItemDefinition", fileName = "NewItem")]
    public class ItemDefinition : ScriptableObject
    {
        /// <summary>Display name shown in the UI.</summary>
        public string ItemName;

        /// <summary>Sprite used both as the placed object and the drag ghost.</summary>
        public Sprite Icon;

        /// <summary>Tile offsets (relative to the anchor tile) that this item covers.</summary>
        public Vector2Int[] Footprint;

        /// <summary>World-space offset applied to the sprite so it centres over its footprint centroid.</summary>
        public Vector2 PivotOffset;
    }
}
