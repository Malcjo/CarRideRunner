using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundMove : MonoBehaviour
{
    // Start is called before the first frame update
    Rigidbody rb;
    public Transform player;
    void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        rb.velocity = new Vector3(player.GetComponent<PlayerController>().GetSpeed(), rb.velocity.y, 0);
    }
}
