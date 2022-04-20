using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildABot {
    public class Spawn : MonoBehaviour
    {
        public Tile basicTile;

        public GameObject tilemap;

        public GameObject room = null;

        void Start()
        {

            int x = (int) transform.position.x;
            int y = (int) transform.position.y;

            Debug.LogFormat("{0}, {1}", x, y);

            // Make a vector 3 to store the position of the object .SetTile is particular
            Vector3Int pos = new Vector3Int(x, y, 0);

            tilemap.GetComponent<Tilemap>().SetTile(pos, basicTile);
        }
    }
}