using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FillRoom : MonoBehaviour
{
    public GameObject[] blocks; // includes whatever blocks you want to spawn

    void Start()
    {
        AddBlocks();
    }
        
    // Functions for adding additional obstacles and powerups
    private void AddBlocks()
    {
        int numOfBlocks = Random.Range(1, 3);

        for (int i = 0; i <= numOfBlocks; i++) {
            if (blocks.Length == 0) continue;
            int rand = Random.Range((int) 0, (int) blocks.Length);

            Debug.LogFormat("{0}, {1}", rand, blocks.Length);

            // int randPos = Random.Range(0, startingPositions.Length);

            GameObject instance = (GameObject) Instantiate(blocks[rand], transform.position, Quaternion.identity);

            instance.transform.parent = transform;
        }
    }
}
