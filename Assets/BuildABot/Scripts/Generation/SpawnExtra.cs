using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot {
    public class SpawnExtra : MonoBehaviour
    {
        public Transform[] startingPositions;
        public GameObject BRICK;

        public LayerMask room;
        private LevelGenerator levelGenerator;

        private GameObject intGrid;

        void Start()
        {
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
            Instantiate(room, transform.position, Quaternion.identity);
        }
    }
}