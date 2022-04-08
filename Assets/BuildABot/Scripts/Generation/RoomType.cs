using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomType : MonoBehaviour
{
    // index 0 --> start, 1 --> RL, index 2--> RBL, index 3 --> TRL, index 4 --> TRBL
    public int type;

    public void RoomDestruction()
    {
        Destroy(gameObject);
    }
}
