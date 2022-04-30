using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot {
    public class FillRoom : MonoBehaviour
    {
        [SerializeField] public bool[] fillQuadrant = new bool[4]; // includes whatever horizontal blocks you want to spawn
        [SerializeField] public bool[] fillQuadrantS = new bool[4]; // includes whatever big blocks you want to spawn
    }
}