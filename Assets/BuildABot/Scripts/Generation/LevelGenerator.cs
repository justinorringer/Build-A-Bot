using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
    A lot of help from this tutorial series by Blackthornprod

    https://www.youtube.com/watch?v=hk6cUanSfXQ
 */
public class LevelGenerator : MonoBehaviour
{
    public Transform[] startingPositions;
    public GameObject[] rooms; 
    // index 0 --> RL, index 1 --> RBL, index 2 --> TRL, index 3 --> TRBL

    public LayerMask room;
    private int gridSize = 4;

    private int direction;
    public float moveAmount;

    private float timeBtwRoom;
    private float startTimeBtwRoom = 0.25f;

    public bool generate = true;

    // variable to prevent down twice
    private int downCounter = 0;

    void Start()
    {
        int randStartingPos = Random.Range(0, startingPositions.Length);
        transform.position = startingPositions[randStartingPos].position;
        Instantiate(rooms[0], transform.position, Quaternion.identity);

        direction = Random.Range(1, 6);
    }

    private void Update()
    {
        if (generate)
        {
            if (timeBtwRoom <= 0) {
                Generate();

                timeBtwRoom = startTimeBtwRoom;
            } else {
                timeBtwRoom -= Time.deltaTime;
            }
        }   
    }

    private void Generate()
    {
        if (direction == 1 || direction == 2) { // Move RIGHT
            if (transform.position.x < (gridSize * moveAmount) - (moveAmount * .5f)) {
                downCounter = 0;
                transform.position = new Vector2(transform.position.x + moveAmount, transform.position.y);
            
                // choose room
                int rand = Random.Range(0, rooms.Length);
                Instantiate(rooms[rand], transform.position, Quaternion.identity);
                // next direction
                direction = Random.Range(1, 6);
                if (direction == 3) {
                    direction = 2;
                } else if (direction == 4) {
                    direction = 5;
                }
            } else {
                direction = 5;
            }
        } else if (direction == 3 || direction == 4) { // Move LEFT
            if (transform.position.x < moveAmount) {
                direction = 5;
            } else {
                downCounter = 0;
                transform.position = new Vector2(transform.position.x - moveAmount, transform.position.y);

                // choose room
                int rand = Random.Range(0, rooms.Length);
                Instantiate(rooms[rand], transform.position, Quaternion.identity);

                direction = Random.Range(3, 6);
            }
        } else if (direction == 5) { // Move DOWN
            downCounter++;
            if (transform.position.y < moveAmount) {
                generate = false;
            } else {
                Collider2D roomDetection = Physics2D.OverlapCircle(transform.position, 1, room);
                if (roomDetection.GetComponent<RoomType>().type != 1 || roomDetection.GetComponent<RoomType>().type != 3) {
                    if (downCounter >= 2) {
                        roomDetection.GetComponent<RoomType>().RoomDestruction();
                        Instantiate(rooms[3], transform.position, Quaternion.identity);
                    }  
                    else {
                        roomDetection.GetComponent<RoomType>().RoomDestruction();

                        int randBottomRoom = Random.Range(1, 4);
                        if (randBottomRoom == 2) randBottomRoom = 1;

                        Instantiate(rooms[randBottomRoom], transform.position, Quaternion.identity);
                    }
                }
                transform.position = new Vector2(transform.position.x, transform.position.y - moveAmount);
            
                int rand = Random.Range(2, 4);

                Instantiate(rooms[rand], transform.position, Quaternion.identity);

                direction = Random.Range(1, 6);
            }
        }
    }

    private void AddEnemies()
    {
        /**
            Generate enemies in the room
        */
    }

    private void AddPowerups()
    {
        /**
            Generate powerups in the room
        */
    }
}
