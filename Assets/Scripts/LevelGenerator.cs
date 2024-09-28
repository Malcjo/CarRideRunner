using System.Collections;
using System.Collections.Generic;  // For Queue
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] platformPrefabs;  // Array of different platform prefabs
    public GameObject obstaclePrefab;     // Prefab of the obstacle (optional)
    public Transform player;              // Reference to the player
    public float spawnDistance = 20f;     // Distance ahead of the player to spawn new platforms
    public float platformLength = 10f;    // Length of each platform
    public float despawnDistance = 30f;   // Distance behind the player to despawn platforms
    private float nextSpawnX = 0f;        // Next X position for spawning

    private Queue<GameObject> spawnedPlatforms = new Queue<GameObject>();  // Queue to track spawned platforms

    void Start()
    {
        // Pre-generate some platforms
        for (int i = 0; i < 5; i++)
        {
            SpawnPlatform(nextSpawnX);
            nextSpawnX += platformLength;
        }
    }

    void Update()
    {
        // Spawn new platform when player is close enough
        if (player.position.x + spawnDistance > nextSpawnX)
        {
            SpawnPlatform(nextSpawnX);
            nextSpawnX += platformLength;
        }

        // Destroy platforms that are behind the player
        if (spawnedPlatforms.Count > 0)
        {
            GameObject oldestPlatform = spawnedPlatforms.Peek();  // Get the oldest platform without removing it

            // If the platform is far behind the player, destroy it
            if (player.position.x - despawnDistance > oldestPlatform.transform.position.x)
            {
                DestroyPlatform();
            }
        }
    }

    void SpawnPlatform(float spawnX)
    {
        // Randomly select a platform prefab from the array
        int randomIndex = Random.Range(0, platformPrefabs.Length);
        GameObject platformPrefab = platformPrefabs[randomIndex];

        // Instantiate the platform
        Vector3 spawnPosition = new Vector3(spawnX, 0, 0);
        GameObject newPlatform = Instantiate(platformPrefab, spawnPosition, Quaternion.identity);

        // Add the newly spawned platform to the queue
        spawnedPlatforms.Enqueue(newPlatform);

        /*
        // Optionally spawn obstacles on the platform
        if (Random.Range(0f, 1f) > 0.5f)  // 50% chance to spawn an obstacle
        {
            Vector3 obstaclePosition = new Vector3(spawnX + Random.Range(2f, 8f), 1f, 0f);
            Instantiate(obstaclePrefab, obstaclePosition, Quaternion.identity);
        }

        */
    }

    void DestroyPlatform()
    {
        // Remove the oldest platform from the queue and destroy it
        GameObject oldestPlatform = spawnedPlatforms.Dequeue();
        Destroy(oldestPlatform);
    }
}
