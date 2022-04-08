using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public GameObject[] objects; // includes regular tile and empty tile for now

    void Start()
    {
        if (objects.Length > 0) {
            int rand = Random.Range(0, objects.Length);

            GameObject instance = (GameObject) Instantiate(objects[rand], transform.position, Quaternion.identity);

            instance.transform.parent = transform;
        }
    }
}
