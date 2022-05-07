using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildABot {
    public class Block : MonoBehaviour
    {
        private GameObject tilemap;

        public RuleTile basicTile;

        [SerializeField] private bool optional = false;

        /**
            Populates the room with tiles on each child's position.
         */
        void Start()
        {
            tilemap = GameObject.Find("Grid");

            foreach (Transform child in transform) {
                if (optional && Random.Range(0, 4) == 0) continue; // for some extra randomness

                int x = (int) child.transform.position.x; // pivot point in bottom corner of each tile
                int y = (int) child.transform.position.y;

                // Make a vector 3 to store the position of the object .SetTile is particular
                Vector3Int pos = new Vector3Int(x, y, 0);

                tilemap.GetComponent<Tilemap>().SetTile(pos, basicTile);
            }

            // DestroyBlock();
        }

        public void DestroyBlock()
        {
            Destroy(gameObject);
        }
    }
}