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

    public LayerPieces[] themes;  // Array of different themes
    private LayerPieces.StageTheme currentStageTheme; // Track the current stage theme
    public int currentLayer = 3;  // Start with High layer (3)
    public int previousLayer = 3; // Track previous layer for transitions
    private float timeSinceLastThemeSwitch = 0f; // Time tracking for theme switching
    private float themeSwitchInterval = 30f; // Time interval before switching themes

    public Transform player;
    public float spawnDistance = 20f;
    public float platformLength = 10f;
    private float nextSpawnX = 0f;

    private bool startRandomSpawn = false;
    private Queue<GameObject> spawnedPlatforms = new Queue<GameObject>();

    private LayerPieces.StageTheme[] currentLayerThemes; // Current layer themes

    public void StartSpawning()
    {
        currentLayerThemes = themes[Random.Range(0, themes.Length)].high;  // Start with a random theme for high layer
        currentStageTheme = currentLayerThemes[Random.Range(0, currentLayerThemes.Length)];
        startRandomSpawn = true;
        nextSpawnX = platformLength * 4 + platformLength / 2;
    }

    void Update()
    {
        if (!startRandomSpawn) return;

        if (player.position.x + spawnDistance > nextSpawnX)
        {
            // Check if a transition is required or continue spawning
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

        // Switch theme after a time interval
        timeSinceLastThemeSwitch += Time.deltaTime;
        if (timeSinceLastThemeSwitch > themeSwitchInterval)
        {
            SwitchThemeWithinLayer();  // Switch theme within the current layer
        }

        // Clean up old platforms
        if (spawnedPlatforms.Count > 0)
        {
            GameObject oldestPlatform = spawnedPlatforms.Peek();
            if (player.position.x - nextSpawnX > oldestPlatform.transform.position.x)
            {
                DestroyPlatform();
            }
        }
    }

    void SpawnPlatform(float spawnX, int layer)
    {
        // Get the correct layer content based on currentLayer
        LayerPieces.StageTheme themeLayer = GetThemeLayer(layer);
        GameObject platformPrefab;
        //float rarityChance = Random.Range(0f, 1f);
        //int rarityChance = Random.Range(0, 4f);
        int rarityChance = 0;

        if (rarityChance == 3 && themeLayer.difficulty.hardestSpecialPieces.Length > 0)
        {
            platformPrefab = themeLayer.difficulty.hardestSpecialPieces[Random.Range(0, themeLayer.difficulty.hardestSpecialPieces.Length)];
        }
        else if (rarityChance == 2 && themeLayer.difficulty.toughSpecialPieces.Length > 0)
        {
            platformPrefab = themeLayer.difficulty.toughSpecialPieces[Random.Range(0, themeLayer.difficulty.toughSpecialPieces.Length)];
        }
        else if (rarityChance == 1 && themeLayer.difficulty.easySpecialPieces.Length > 0)
        {
            platformPrefab = themeLayer.difficulty.easySpecialPieces[Random.Range(0, themeLayer.difficulty.easySpecialPieces.Length)];
        }
        else
        {
            platformPrefab = themeLayer.difficulty.standardPieces[Random.Range(0, themeLayer.difficulty.standardPieces.Length)];
        }

        Vector3 spawnPosition = new Vector3(spawnX, 0, 0);
        GameObject newPlatform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);
        spawnedPlatforms.Enqueue(newPlatform);
    }

    void SwitchThemeWithinLayer()
    {
        // Switch the theme but stay within the same layer
        LayerPieces.StageTheme[] currentLayerOptions = GetLayerThemes(currentLayer);
        currentStageTheme = currentLayerOptions[Random.Range(0, currentLayerOptions.Length)];

        timeSinceLastThemeSwitch = 0f;  // Reset the theme switch timer
        Debug.Log($"Switched to a new theme in layer {currentLayer}");
    }

    LayerPieces.StageTheme[] GetLayerThemes(int layer)
    {
        switch (layer)
        {
            case 4:
                return themes[Random.Range(0, themes.Length)].sky;  // Return Sky themes
            case 3:
                return themes[Random.Range(0, themes.Length)].high;  // Return High themes
            case 2:
                return themes[Random.Range(0, themes.Length)].medium;  // Return Medium themes
            case 1:
                return themes[Random.Range(0, themes.Length)].lower;  // Return Lower themes
            default:
                return themes[Random.Range(0, themes.Length)].high;  // Default to High themes
        }
    }

    LayerPieces.StageTheme GetThemeLayer(int layer)
    {
        // Return the current theme for the selected layer
        return currentStageTheme;
    }

    void TransitionToLayer(int nextLayer)
    {
        // Get the current layer's themes
        LayerPieces.StageTheme[] nextLayerThemes = GetLayerThemes(nextLayer);

        // Select a random theme from the new layer's themes
        LayerPieces.StageTheme nextLayerTheme = nextLayerThemes[Random.Range(0, nextLayerThemes.Length)];
        GameObject enterPiece;

        // Determine if the player is moving up or down between layers
        if (nextLayer > currentLayer)
        {
            // Moving up a layer, get enterFromBelowPieces
            enterPiece = nextLayerTheme.transitionPieces.enterFromBelowPieces[Random.Range(0, nextLayerTheme.transitionPieces.enterFromBelowPieces.Length)];
        }
        else
        {
            // Moving down a layer, get enterFromAbovePieces
            enterPiece = nextLayerTheme.transitionPieces.enterFromAbovePieces[Random.Range(0, nextLayerTheme.transitionPieces.enterFromAbovePieces.Length)];
        }

        // Instantiate the transition piece
        Instantiate(enterPiece, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);
        nextSpawnX += platformLength;

        // Update current and previous layers
        previousLayer = currentLayer;
        currentLayer = nextLayer;

        // Update the theme based on the new layer
        currentLayerThemes = nextLayerThemes;  // Update the layer's themes
        currentStageTheme = nextLayerTheme;    // Update to the new stage theme
    }

    void DestroyPlatform()
    {
        GameObject oldestPlatform = spawnedPlatforms.Dequeue();
        Destroy(oldestPlatform);
    }

    bool ShouldTransitionLayer()
    {
        // Random chance to transition layers
        return Random.Range(0f, 1f) < 0.2f;  // 20% chance to transition layers
    }

    int GetValidAdjacentLayer()
    {
        List<int> validLayers = new List<int>();

        // Transition between High (3) and Sky (4) for now
        if (currentLayer == 4)
        {
            validLayers.Add(3);  // Can move down to High
        }
        else if (currentLayer == 3)
        {
            validLayers.Add(4);  // Can move up to Sky
        }

        return validLayers[Random.Range(0, validLayers.Count)];
    }
}
