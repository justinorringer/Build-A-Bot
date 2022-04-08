using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnExtra : MonoBehaviour
{
    public Transform[] startingPositions;
    public GameObject BRICK;

    public LayerMask room;
    public LevelGenerator levelGeneration;

    // Update is called once per frame
    void Update()
    {
        Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, room);

        if (roomDetection == null && !levelGeneration.generate) {
            // Spawn full for now) room
            Instantiate(BRICK, transform.position, Quaternion.identity);
            Destroy(gameObject);
        }    
    }
}
