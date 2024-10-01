using UnityEngine;
using UnityEditor;

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

    // Function to toggle MeshRenderer for all objects with the "Hitbox" layer
    void ToggleHitboxMeshRenderers()
    {
        // Get all game objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Loop through and find objects with the "Hitbox" layer
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Hitbox"))
            {
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // Toggle the renderer's enabled state
                    renderer.enabled = !meshRenderersOn;
                }
            }
        }

        // Flip the state of meshRenderersOn after toggling
        meshRenderersOn = !meshRenderersOn;
    }

    // Function to toggle MeshRenderer for all objects with the "Hitbox" layer and "Killbox" tag
    void ToggleKillboxMeshRenderers()
    {
        // Get all game objects in the scene
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        // Loop through and find objects with the "Hitbox" layer and "Killbox" tag
        foreach (GameObject obj in allObjects)
        {
            if (obj.layer == LayerMask.NameToLayer("Hitbox") && obj.CompareTag("Killbox"))
            {
                MeshRenderer renderer = obj.GetComponent<MeshRenderer>();
                if (renderer != null)
                {
                    // Toggle the renderer's enabled state
                    renderer.enabled = !killboxRenderersOn;
                }
            }
        }

        // Flip the state of killboxRenderersOn after toggling
        killboxRenderersOn = !killboxRenderersOn;
    }
}
