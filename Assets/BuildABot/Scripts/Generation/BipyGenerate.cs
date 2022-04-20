using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot {
    public class BipyGenerate : MonoBehaviour
    {
        public GameObject Bipy;

        private LevelGenerator levelGenerator;

        void Start() {
            levelGenerator = GameObject.Find("Generator").GetComponent<LevelGenerator>();
        }

        void Update() {
            if (!levelGenerator.generate) {
                Instantiate(Bipy, transform.position, Quaternion.identity);
                Destroy(gameObject);
            }
        }
    }
}