using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnEntity : MonoBehaviour
{
    public GameObject[] objects; // includes regular tile and empty tile for now

    private LevelGenerator levelGenerator;

    void Start() {
        levelGenerator = GameObject.Find("Generator").GetComponent<LevelGenerator>();
    }
    // Update is called once per frame
    void Update()
    {
        if (!levelGenerator.generate) {
            Instantiate(objects[0], transform.position, Quaternion.identity);
            Destroy(gameObject);
        }
    }
}