using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    // index 0 --> RL, index 1 --> RBL, index 2 --> TRL, index 3 --> TRBL
    public int type;

    public void RoomDestruction()
    {
        Destroy(gameObject);
    }
}
