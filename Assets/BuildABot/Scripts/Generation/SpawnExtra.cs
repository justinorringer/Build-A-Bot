using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnExtra : MonoBehaviour
{
    public Transform[] startingPositions;
    public GameObject BRICK;

    public LayerMask room;
    private LevelGenerator levelGenerator;

    private GameObject intGrid;

    void Start()
    {
        intGrid = GameObject.Find("IntGrid"); // IntGrid is the parent of all the rooms
        levelGenerator = GameObject.Find("Generator").GetComponent<LevelGenerator>();
    }

    // Update is called once per frame
    void Update()
    {
        Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, room);

        if (!levelGenerator.generate) {
            // Spawn full for now) room
            if (roomDetection == null) {
                InstantiateRoom(BRICK);
            }
            Destroy(gameObject);
        }
    }

    private void InstantiateRoom(GameObject room) {
        GameObject r = (GameObject) Instantiate(room, transform.position, Quaternion.identity);

        r.transform.parent = intGrid.transform;
    }
}
