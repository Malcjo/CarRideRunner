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

        public TransitionPieces transitionPieces;
        public LayerContentPieces layerContentPieces;
    }

    public LayerPieces skyPieces;
    public LayerPieces highPieces;
    public LayerPieces mediumPieces;  // Disabled for now
    public LayerPieces lowerPieces;   // Disabled for now

    private LayerPieces[] allLayerPieces;

    public Transform player;
    public float spawnDistance = 20f;
    public float platformLength = 10f;
    public float despawnDistance = 30f;
    private float nextSpawnX = 0f;

    private bool startRandomSpawn = false;
    private Queue<GameObject> spawnedPlatforms = new Queue<GameObject>();

    private int currentLayer = 3;  // Start with High layer (3)
    private int previousLayer = 3; // Start with High as the previous layer too

    public void StartSpawning()
    {
        // Limit to High and Sky for now
        allLayerPieces = new LayerPieces[] { lowerPieces, mediumPieces, highPieces, skyPieces };
        startRandomSpawn = true;
        nextSpawnX = platformLength * 4 + platformLength / 2;
    }

    void Update()
    {
        if (!startRandomSpawn) return;

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
        LayerPieces layerPieces = allLayerPieces[layer - 1]; // Adjust for 1-based layer index

        GameObject platformPrefab;

        float rarityChance = Random.Range(0f, 1f);
        if (rarityChance < 0.1f && layerPieces.layerContentPieces.hardestSpecialPieces.Length > 0)
        {
            platformPrefab = layerPieces.layerContentPieces.hardestSpecialPieces[Random.Range(0, layerPieces.layerContentPieces.hardestSpecialPieces.Length)];
        }
        else if (rarityChance < 0.3f && layerPieces.layerContentPieces.toughSpecialPieces.Length > 0)
        {
            platformPrefab = layerPieces.layerContentPieces.toughSpecialPieces[Random.Range(0, layerPieces.layerContentPieces.toughSpecialPieces.Length)];
        }
        else if (rarityChance < 0.6f && layerPieces.layerContentPieces.easySpecialPieces.Length > 0)
        {
            platformPrefab = layerPieces.layerContentPieces.easySpecialPieces[Random.Range(0, layerPieces.layerContentPieces.easySpecialPieces.Length)];
        }
        else
        {
            platformPrefab = layerPieces.layerContentPieces.standardPieces[Random.Range(0, layerPieces.layerContentPieces.standardPieces.Length)];
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
    /*
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
            validLayers.Add(2);  // Can move to Medium
        }
        else if (currentLayer == 2)  // Medium layer
        {
            validLayers.Add(3);  // Can move to High
            validLayers.Add(1);  // Can move to Lower
        }
        else if (currentLayer == 1)  // Lower layer
        {
            validLayers.Add(2);  // Can move to Medium
        }

        return validLayers[Random.Range(0, validLayers.Count)];
    }
    */

    void TransitionToLayer(int nextLayer)
    {
        LayerPieces nextLayerPieces = allLayerPieces[nextLayer - 1];
        GameObject enterPiece;

        // Check if moving to a higher layer or lower layer
        if (nextLayer > currentLayer)  // Moving up a layer
        {
            enterPiece = nextLayerPieces.transitionPieces.enterFromBelowPieces[Random.Range(0, nextLayerPieces.transitionPieces.enterFromBelowPieces.Length)];
        }
        else  // Moving down a layer
        {
            enterPiece = nextLayerPieces.transitionPieces.enterFromAbovePieces[Random.Range(0, nextLayerPieces.transitionPieces.enterFromAbovePieces.Length)];
        }

        // Instantiate the transition piece
        Instantiate(enterPiece, new Vector3(nextSpawnX, 0, 0), Quaternion.identity);
        nextSpawnX += platformLength;

        // Update current and previous layers
        previousLayer = currentLayer;
        currentLayer = nextLayer;
    }

    void DestroyPlatform()
    {
        GameObject oldestPlatform = spawnedPlatforms.Dequeue();
        Destroy(oldestPlatform);
    }

}
