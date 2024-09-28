using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform player;  // Reference to the player
    public Vector3 offset = new Vector3(0, 2, -10);  // Offset from the player

    void Update()
    {
        // Follow the player only on the X-axis
        transform.position = new Vector3(player.position.x + offset.x, player.position.y + offset.y, offset.z);
    }
}
