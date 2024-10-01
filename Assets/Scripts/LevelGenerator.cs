using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelGenerator : MonoBehaviour
{
    public GameObject[] platformPrefabs;  // Array of different platform prefabs to spawn randomly
    public Transform player;              // Reference to the player
    public float spawnDistance = 20f;     // Distance ahead of the player to spawn new platforms
    public float platformLength = 10f;    // Length of each platform
    public float despawnDistance = 30f;   // Distance behind the player to despawn platforms
    private float nextSpawnX = 0f;        // Next X position for spawning

    private bool startRandomSpawn = false;  // Flag to indicate if random spawning has started
    private Queue<GameObject> spawnedPlatforms = new Queue<GameObject>();  // Queue to track spawned platforms

    
    public void startSpawning()
    {
        startRandomSpawn = true;
        nextSpawnX = platformLength + (platformLength /2);
        //nextSpawnX = Mathf.Ceil(player.position.x / platformLength) * platformLength;
        //nextSpawnX = player.position.x + (platformLength /2);  // Set spawn position after the custom section
    }
    void Update()
    {
        // If random spawning hasn't started yet, exit
        if (!startRandomSpawn)
        {
            return;
        }

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
    }

    void DestroyPlatform()
    {
        // Remove the oldest platform from the queue and destroy it
        GameObject oldestPlatform = spawnedPlatforms.Dequeue();
        Destroy(oldestPlatform);
    }

    // Trigger event to start random spawning
    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            // When the player hits the trigger, start random spawning
            startRandomSpawn = true;

            // Set the starting position for random spawning after the custom section
            nextSpawnX = player.position.x + platformLength;  // Begin spawning after current position
        }
    }
}
