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
                Vector3 position = transform.position;
                Player player = GameManager.GetPlayer();
                if (player == null)
                {
                    Vector3 p = new Vector3(position.x + pos[0], position.y + pos[1], 0);
                    Instantiate(Bipy, p, Quaternion.identity);
                }
                else
                {
                    Vector3 p = new Vector3(position.x + pos[0], position.y + pos[1], 0);
                    player.gameObject.transform.position = p;
                    player.CharacterMovement.ClearMovement();
                }
                Destroy(gameObject);
            }
        }
    }
}