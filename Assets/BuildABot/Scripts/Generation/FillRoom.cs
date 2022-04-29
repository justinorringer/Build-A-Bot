using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot {
    public class FillRoom : MonoBehaviour
    {
        public GameObject[] horizontal; // includes whatever horizontal blocks you want to spawn

        public GameObject[] vertical; // includes whatever vertical blocks you want to spawn
        void Start()
        {
            AddHorizontal();
            AddVertical();
        }
            
        // Functions for adding additional obstacles and powerups
        private void AddHorizontal()
        {
            int numOfBlocks = Random.Range(1, 3);
 
            for (int i = 0; i <= numOfBlocks; i++) {
                if (horizontal.Length == 0) continue;
                int rand = Random.Range((int) 0, (int) horizontal.Length);

                Instantiate(horizontal[rand], transform.position, Quaternion.identity);
            }
        }

        private void AddVertical()
        {
            int rand = Random.Range(0, vertical.Length);
 
            if (vertical.Length > 0) Instantiate(vertical[rand], transform.position, Quaternion.identity);
        }
    }
}