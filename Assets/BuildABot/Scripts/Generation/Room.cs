using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildABot {
    public class Room : MonoBehaviour
    {
        // index 0 --> start, 1 --> RL, index 2--> RBL, index 3 --> TRL, index 4 --> TRBL
        public int type;

        private GameObject tilemap;

        public Tile basicTile;

        /**
            Populates the room with tiles on each child's position.
         */
        void Start()
        {
            tilemap = GameObject.Find("Grid");

            foreach (Transform child in transform) {
                int x = (int) child.transform.position.x; // pivot point in bottom corner of each tile
                int y = (int) child.transform.position.y;

                Debug.LogFormat("{0}, {1}", x, y);

                // Make a vector 3 to store the position of the object .SetTile is particular
                Vector3Int pos = new Vector3Int(x, y, 0);

                tilemap.GetComponent<Tilemap>().SetTile(pos, basicTile);
            }
        }

        public void DestroyRoom()
        {
            Destroy(gameObject);
        }
    }
}