using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildABot {
    public enum RoomType
    {
        START,
        RL,
        RBL,
        TRL,
        TRBL,
        END,
        LEND,
        REND,
        NPC,
        NONE
    }
    public class RoomTemplate : MonoBehaviour
    {
        // index 0 --> start, 1 --> RL, index 2--> RBL, index 3 --> TRL, index 4 --> TRBL
        public RoomType type;

        public GameObject[] rooms;

        /**
            Chooses which room to instantiate based on the type.
         */
        void Start()
        {
            int randRoom = Random.Range(0, rooms.Length); // choose a random room

            GameObject room = (GameObject)Instantiate(rooms[randRoom], transform.position, Quaternion.identity); // instantiate that room

            room.transform.parent = transform.parent; // make the room a child of the grid

            DestroyRoom();
        }

        public void DestroyRoom()
        {
            Destroy(gameObject);
        }
    }
}