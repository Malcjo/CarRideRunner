using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    [System.Serializable]
    public class LayerPieces
    {
        [System.Serializable]
        public class TransitionPieces
        {
            public GameObject[] enterFromAbovePieces;  // Pieces for entering from above
            public GameObject[] enterFromBelowPieces;  // Pieces for entering from below
        }

        [System.Serializable]
        public class LayerContentPieces
        {
            public GameObject[] standardPieces;
            public GameObject[] easySpecialPieces;
            public GameObject[] toughSpecialPieces;
            public GameObject[] hardestSpecialPieces;
        }

        [System.Serializable]
        public class StageTheme
        {
            public LayerContentPieces difficulty;  // Themed pieces for each layer
            public TransitionPieces transitionPieces;   // Transition pieces for the theme
        }

        public StageTheme[] lower;
        public StageTheme[] medium;
        public StageTheme[] high;
        public StageTheme[] sky;
    }

    public LayerPieces[] themes;  // Multiple themes to choose from

    private LayerPieces.StageTheme currentStageTheme;  // The current active stage theme
    public Transform player;
    public float spawnDistance = 20f;
    public float platformLength = 10f;
    public float despawnDistance = 30f;
    private float nextSpawnX = 0f;

    private bool startRandomSpawn = false;
    private Queue<GameObject> spawnedPlatforms = new Queue<GameObject>();

    private int currentLayer = 3;  // Start with High layer (3)
    private int previousLayer = 3; // Start with High as the previous layer too

    // Timer or distance for theme switching
    private float themeSwitchInterval = 30f;  // Time or distance interval for theme switching
    private float timeSinceLastThemeSwitch = 0f;

    public void StartSpawning()
    {
        // Set up with the first theme and the high layer
        currentStageTheme = themes[0].high[0];  // Example: Start with the first theme's high layer
        startRandomSpawn = true;
        nextSpawnX = platformLength * 4 + platformLength / 2;
    }

    void Update()
    {
        if (!startRandomSpawn) return;

        // Check for theme switching based on time or distance
        timeSinceLastThemeSwitch += Time.deltaTime;
        if (timeSinceLastThemeSwitch >= themeSwitchInterval)
        {
            SwitchTheme();
        }

        // Handle platform spawning
        if (player.position.x + spawnDistance > nextSpawnX)
        {
            if (ShouldTransitionLayer())
            {
                int nextLayer = GetValidAdjacentLayer();
                TransitionToLayer(nextLayer);
            }
            else
            {
                SpawnPlatform(nextSpawnX, currentLayer);
                nextSpawnX += platformLength;
            }
        }

        // Destroy platforms behind the player
        if (spawnedPlatforms.Count > 0)
        {
            GameObject oldestPlatform = spawnedPlatforms.Peek();
            if (player.position.x - despawnDistance > oldestPlatform.transform.position.x)
            {
                DestroyPlatform();
            }
        }
    }

    void SpawnPlatform(float spawnX, int layer)
    {
        // Access the current stage's difficulty for the chosen layer
        LayerPieces.LayerContentPieces layerContentPieces = currentStageTheme.difficulty;

        GameObject platformPrefab;

        float rarityChance = Random.Range(0f, 1f);
        if (rarityChance < 0.1f && layerContentPieces.hardestSpecialPieces.Length > 0)
        {
            platformPrefab = layerContentPieces.hardestSpecialPieces[Random.Range(0, layerContentPieces.hardestSpecialPieces.Length)];
        }
        else if (rarityChance < 0.3f && layerContentPieces.toughSpecialPieces.Length > 0)
        {
            platformPrefab = layerContentPieces.toughSpecialPieces[Random.Range(0, layerContentPieces.toughSpecialPieces.Length)];
        }
        else if (rarityChance < 0.6f && layerContentPieces.easySpecialPieces.Length > 0)
        {
            platformPrefab = layerContentPieces.easySpecialPieces[Random.Range(0, layerContentPieces.easySpecialPieces.Length)];
        }
        else
        {
            platformPrefab = layerContentPieces.standardPieces[Random.Range(0, layerContentPieces.standardPieces.Length)];
        }

        Vector3 spawnPosition = new Vector3(spawnX, 0, 0);
        GameObject newPlatform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
        spawnedPlatforms.Enqueue(newPlatform);
    }

    bool ShouldTransitionLayer()
    {
        return Random.Range(0f, 1f) < 0.2f;  // 20% chance to transition
    }

    int GetValidAdjacentLayer()
    {
        List<int> validLayers = new List<int>();

        // Determine adjacent valid layers
        if (currentLayer == 4)  // Sky layer
        {
            validLayers.Add(3);  // Can move to High
        }
        else if (currentLayer == 3)  // High layer
        {
            validLayers.Add(4);  // Can move to Sky
        }

        return validLayers[Random.Range(0, validLayers.Count)];
    }

    void TransitionToLayer(int nextLayer)
    {
        LayerPieces.StageTheme nextLayerTheme = null;  // Initialize variable to avoid unassigned variable error

        // Get the next layer theme
        if (nextLayer == 4)
        {
            nextLayerTheme = themes[Random.Range(0, themes.Length)].sky[Random.Range(0, themes[0].sky.Length)];
        }
        else if (nextLayer == 3)
        {
            nextLayerTheme = themes[Random.Range(0, themes.Length)].high[Random.Range(0, themes[0].high.Length)];
        }

        GameObject enterPiece;

        // Check if moving to a higher or lower layer
        if (nextLayer > currentLayer)  // Moving up a layer
        {
            enterPiece = nextLayerTheme.transitionPieces.enterFromBelowPieces[Random.Range(0, nextLayerTheme.transitionPieces.enterFromBelowPieces.Length)];
        }
        else  // Moving down a layer
        {
            enterPiece = nextLayerTheme.transitionPieces.enterFromAbovePieces[Random.Range(0, nextLayerTheme.transitionPieces.enterFromAbovePieces.Length)];
        }

        // Instantiate the transition piece
        Instantiate(enterPiece, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);
        nextSpawnX += platformLength;

        // Update current and previous layers
        previousLayer = currentLayer;
        currentLayer = nextLayer;

        // Set the current stage theme to the next layer theme
        currentStageTheme = nextLayerTheme;
    }

    void DestroyPlatform()
    {
        GameObject oldestPlatform = spawnedPlatforms.Dequeue();
        Destroy(oldestPlatform);
    }

    // Switch to a new theme randomly
    void SwitchTheme()
    {
        // Randomly choose a new theme
        currentStageTheme = themes[Random.Range(0, themes.Length)].high[Random.Range(0, themes[0].high.Length)]; // Example: switch to the high layer of the new theme

        // Reset the timer for theme switching
        timeSinceLastThemeSwitch = 0f;
        Debug.Log("Switched to new theme");
    }
}
