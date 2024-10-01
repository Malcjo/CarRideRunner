using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

public class HitboxVisibilityTool : EditorWindow
{
    private bool meshRenderersOn = false;  // Track if the renderers are on or off
    private bool killboxRenderersOn = false;  // Track if killbox renderers are on or off

    // Add menu item to show this window in Unity's "Window" menu
    [MenuItem("Tools/Hitbox Visibility Tool")]
    public static void ShowWindow()
    {
        GetWindow<HitboxVisibilityTool>("Hitbox Visibility Tool");
    }

    void OnGUI()
    {
        GUILayout.Label("Hitbox MeshRenderer Toggle Tool", EditorStyles.boldLabel);

        // Button to toggle MeshRenderer for all objects with the "Hitbox" layer
        if (GUILayout.Button("Toggle Hitbox Mesh Renderers"))
        {
            ToggleHitboxMeshRenderers();
        }

        // Button to toggle MeshRenderer for objects with the "Hitbox" layer and "Killbox" tag
        if (GUILayout.Button("Toggle Killbox Mesh Renderers"))
        {
            ToggleKillboxMeshRenderers();
        }
    }

    // Function to toggle MeshRenderer for all prefabs and non-prefabs with the "Hitbox" layer
    void ToggleHitboxMeshRenderers()
    {
        // Step 1: Modify prefabs
        string[] allPrefabPaths = GetAllPrefabPaths();

        foreach (string prefabPath in allPrefabPaths)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            // Loop through and find objects with the "Hitbox" layer using Transform
            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (child.gameObject.layer == LayerMask.NameToLayer("Hitbox"))
                {
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        // Toggle the renderer's enabled state
                        renderer.enabled = !meshRenderersOn;
                    }
                }
            }

            // Save changes to the prefab
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);

            // Update prefab instances in the scene
            UpdatePrefabInstancesInScene(prefabPath);
        }

        // Step 2: Modify non-prefab objects in the scene
        GameObject[] allSceneObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allSceneObjects)
        {
            // Only apply to objects that are not connected to a prefab
            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab)
            {
                if (obj.layer == LayerMask.NameToLayer("Hitbox"))
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = !meshRenderersOn;
                    }
                }
            }
        }

        // Flip the state of meshRenderersOn after toggling
        meshRenderersOn = !meshRenderersOn;
    }

    // Function to toggle MeshRenderer for all prefabs and non-prefabs with the "Hitbox" layer and "Killbox" tag
    void ToggleKillboxMeshRenderers()
    {
        // Step 1: Modify prefabs
        string[] allPrefabPaths = GetAllPrefabPaths();

        foreach (string prefabPath in allPrefabPaths)
        {
            GameObject prefab = PrefabUtility.LoadPrefabContents(prefabPath);

            // Loop through and find objects with the "Hitbox" layer and "Killbox" tag
            foreach (Transform child in prefab.GetComponentsInChildren<Transform>(true))
            {
                if (child.gameObject.layer == LayerMask.NameToLayer("Hitbox") && child.CompareTag("Killbox"))
                {
                    MeshRenderer renderer = child.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        // Toggle the renderer's enabled state
                        renderer.enabled = !killboxRenderersOn;
                    }
                }
            }

            // Save changes to the prefab
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            PrefabUtility.UnloadPrefabContents(prefab);

            // Update prefab instances in the scene
            UpdatePrefabInstancesInScene(prefabPath);
        }

        // Step 2: Modify non-prefab objects in the scene
        GameObject[] allSceneObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allSceneObjects)
        {
            // Only apply to objects that are not connected to a prefab
            if (PrefabUtility.GetPrefabInstanceStatus(obj) == PrefabInstanceStatus.NotAPrefab)
            {
                if (obj.layer == LayerMask.NameToLayer("Hitbox") && obj.CompareTag("Killbox"))
                {
                    MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        renderer.enabled = !killboxRenderersOn;
                    }
                }
            }
        }

        // Flip the state of killboxRenderersOn after toggling
        killboxRenderersOn = !killboxRenderersOn;
    }

    // Helper function to retrieve all prefab paths in the project
    string[] GetAllPrefabPaths()
    {
        // Find all prefabs in the project
        string[] guids = AssetDatabase.FindAssets("t:Prefab");
        string[] prefabPaths = new string[guids.Length];

        for (int i = 0; i < guids.Length; i++)
        {
            prefabPaths[i] = AssetDatabase.GUIDToAssetPath(guids[i]);
        }

        return prefabPaths;
    }

    // Helper function to update prefab instances in the scene
    void UpdatePrefabInstancesInScene(string prefabPath)
    {
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        GameObject[] prefabInstances = GameObject.FindObjectsOfType<GameObject>();

        foreach (GameObject instance in prefabInstances)
        {
            if (PrefabUtility.GetPrefabInstanceStatus(instance) == PrefabInstanceStatus.Connected)
            {
                GameObject prefabSource = PrefabUtility.GetCorrespondingObjectFromSource(instance);
                if (prefabSource == prefab)
                {
                    PrefabUtility.ApplyPrefabInstance(instance, InteractionMode.UserAction);
                    EditorSceneManager.MarkSceneDirty(instance.scene);
                }
            }
        }
    }
}
