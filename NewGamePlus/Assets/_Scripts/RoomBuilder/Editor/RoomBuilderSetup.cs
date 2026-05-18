using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace Harmonize.RoomBuilder.Editor
{
    /// <summary>
    /// One-shot editor utility: creates the DefaultGrid and Wardrobe assets,
    /// then builds the RoomBuilder scene wired up and ready to play.
    /// Run via Tools > Harmonize > Setup Room Builder.
    /// </summary>
    public static class RoomBuilderSetup
    {
        private const string GridAssetPath    = "Assets/_Data/Grid/DefaultGrid.asset";
        private const string WardrobeAssetPath = "Assets/_Data/Items/Wardrobe.asset";
        private const string ScenePath        = "Assets/Scenes/RoomBuilder.unity";

        private const float CameraOrthographicSize = 5f;
        private const float CameraNearClip = 0.3f;
        private const float CameraFarClip  = 1000f;
        private const float CameraBgBrightness = 0.15f;
        private const float CameraBgBlue = 0.2f;
        private static readonly Vector3 CameraPosition = new Vector3(0f, 2f, -10f);
        private static readonly Quaternion LightRotation = Quaternion.Euler(50f, -30f, 0f);

        [MenuItem("Tools/Harmonize/Setup Room Builder")]
        public static void Run()
        {
            CreateFolders();
            GridManagerSO grid = CreateOrLoadGridAsset();
            ItemDefinition wardrobe = CreateOrLoadWardrobeAsset();
            BuildScene(grid, wardrobe);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            Debug.Log("[RoomBuilderSetup] Done. Open Assets/Scenes/RoomBuilder.unity to play.");
        }

        private static void CreateFolders()
        {
            EnsureFolder("Assets/_Data");
            EnsureFolder("Assets/_Data/Grid");
            EnsureFolder("Assets/_Data/Items");
        }

        private static void EnsureFolder(string path)
        {
            if (!AssetDatabase.IsValidFolder(path))
            {
                string parent = Path.GetDirectoryName(path).Replace('\\', '/');
                string folder = Path.GetFileName(path);
                AssetDatabase.CreateFolder(parent, folder);
            }
        }

        private static GridManagerSO CreateOrLoadGridAsset()
        {
            GridManagerSO existing = AssetDatabase.LoadAssetAtPath<GridManagerSO>(GridAssetPath);
            if (existing != null) return existing;

            GridManagerSO asset = ScriptableObject.CreateInstance<GridManagerSO>();
            asset.Initialize();
            AssetDatabase.CreateAsset(asset, GridAssetPath);
            return asset;
        }

        private static ItemDefinition CreateOrLoadWardrobeAsset()
        {
            ItemDefinition existing = AssetDatabase.LoadAssetAtPath<ItemDefinition>(WardrobeAssetPath);
            if (existing != null) return existing;

            ItemDefinition asset = ScriptableObject.CreateInstance<ItemDefinition>();
            asset.ItemName = "Wardrobe";
            asset.Footprint = new[] { Vector2Int.zero };
            asset.PivotOffset = Vector2.zero;
            AssetDatabase.CreateAsset(asset, WardrobeAssetPath);
            return asset;
        }

        private static void BuildScene(GridManagerSO grid, ItemDefinition wardrobe)
        {
            Scene scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // ── Camera ──────────────────────────────────────────────────────────────
            GameObject cameraGO = new GameObject("Main Camera");
            cameraGO.tag = "MainCamera";

            Camera cam = cameraGO.AddComponent<Camera>();
            cam.orthographic = true;
            cam.orthographicSize = CameraOrthographicSize;
            cam.nearClipPlane = CameraNearClip;
            cam.farClipPlane = CameraFarClip;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = new Color(CameraBgBrightness, CameraBgBrightness, CameraBgBlue, 1f);
            cameraGO.transform.position = CameraPosition;

            cameraGO.AddComponent<AudioListener>();
            cameraGO.AddComponent<CameraController>();

            // URP additional camera data
            var urpData = cameraGO.AddComponent<UniversalAdditionalCameraData>();
            urpData.renderPostProcessing = false;

            // ── Grid Manager GO ─────────────────────────────────────────────────────
            GameObject gridGO = new GameObject("GridManager");
            GridRenderer renderer = gridGO.AddComponent<GridRenderer>();

            // Assign the GridManagerSO via serialized property so it sticks.
            SerializedObject soRenderer = new SerializedObject(renderer);
            soRenderer.FindProperty("_gridManager").objectReferenceValue = grid;
            soRenderer.ApplyModifiedPropertiesWithoutUndo();

            // ── Placement System GO ─────────────────────────────────────────────────
            GameObject placementGO = new GameObject("PlacementSystem");
            PlacementSystem placement = placementGO.AddComponent<PlacementSystem>();

            SerializedObject soPlacement = new SerializedObject(placement);
            soPlacement.FindProperty("_gridManager").objectReferenceValue = grid;
            soPlacement.FindProperty("_gridRenderer").objectReferenceValue = renderer;
            soPlacement.FindProperty("_camera").objectReferenceValue = cam;
            soPlacement.ApplyModifiedPropertiesWithoutUndo();

            // ── Directional Light ───────────────────────────────────────────────────
            GameObject lightGO = new GameObject("Directional Light");
            Light light = lightGO.AddComponent<Light>();
            light.type = LightType.Directional;
            light.intensity = 1f;
            lightGO.transform.rotation = LightRotation;

            EditorSceneManager.SaveScene(scene, ScenePath);
        }
    }
}
