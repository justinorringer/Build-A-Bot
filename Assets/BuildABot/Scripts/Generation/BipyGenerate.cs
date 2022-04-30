using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot {
    public class BipyGenerate : MonoBehaviour
    {
        public GameObject Bipy;

        private LevelGenerator levelGenerator;

        [SerializeField] private int[] pos = new int[2] { 10, 10 };

        void Start() {
            levelGenerator = GameObject.Find("Generator").GetComponent<LevelGenerator>();
        }

        void Update() {
            if (!levelGenerator.generate) {
                Vector3 pos = new Vector3(transform.position.x + this.pos[0], transform.position.y + this.pos[1], 0);
                Instantiate(Bipy, pos, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}