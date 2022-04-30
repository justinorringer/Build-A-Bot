using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildABot {
    public enum RoomType
    {
        LSTART,
        RSTART,
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
        public RoomType type;

        public GameObject[] rooms;

        public Block[] q1;
        public Block[] q2;
        public Block[] q3;
        public Block[] q4;

        public Block[] qs1;
        public Block[] qs2;
        public Block[] qs3;
        public Block[] qs4;

        FillRoom _shortcutToFillRoom;
        /**
            Chooses which room to instantiate based on the type.
         */
        void Start()
        {
            int randRoom = Random.Range(0, rooms.Length); // choose a random room

            GameObject room = (GameObject)Instantiate(rooms[randRoom], transform.position, Quaternion.identity); // instantiate that room

            room.transform.parent = transform.parent; // make the room a child of the grid

            if (room.GetComponent<FillRoom>() != null)
            {
                _shortcutToFillRoom = room.GetComponent<FillRoom>();

                InstantiateBlocks(room);
            }

            Utility.DelayedFunction(this, 0.1f, () => {
                DestroyRoom();
            });
        }

        public void DestroyRoom()
        {
            Destroy(gameObject);
        }

        private void InstantiateBlocks(GameObject room) {
            bool[] fillQ = _shortcutToFillRoom.fillQuadrant;
            bool[] fillQS = _shortcutToFillRoom.fillQuadrantS;

            Vector3 pos = new Vector3(transform.position.x, transform.position.y, 0);

            if (fillQ[0] && q1.Length > 0)
            {
                Instantiate(q1[Random.Range(0, q1.Length)], pos, Quaternion.identity);
            }
            if (fillQ[1] && q2.Length > 0)
            {
                Instantiate(q2[Random.Range(0, q2.Length)], pos, Quaternion.identity);
            }
            if (fillQ[2] && q3.Length > 0)
            {
                Instantiate(q3[Random.Range(0, q3.Length)], pos, Quaternion.identity);
            }
            if (fillQ[3] && q4.Length > 0)
            {
                Instantiate(q4[Random.Range(0, q4.Length)], pos, Quaternion.identity);
            }

            if (fillQS[0] && qs1.Length > 0)
            {
                Instantiate(qs1[Random.Range(0, qs1.Length)], pos, Quaternion.identity);
            }
            if (fillQS[1] && qs2.Length > 0)
            {
                Instantiate(qs2[Random.Range(0, qs2.Length)], pos, Quaternion.identity);
            }
            if (fillQS[2] && qs3.Length > 0)
            {
                Instantiate(qs3[Random.Range(0, qs3.Length)], pos, Quaternion.identity);
            }
            if (fillQS[3] && qs4.Length > 0)
            {
                Instantiate(qs4[Random.Range(0, qs4.Length)], pos, Quaternion.identity);
            }
        }
    }
}