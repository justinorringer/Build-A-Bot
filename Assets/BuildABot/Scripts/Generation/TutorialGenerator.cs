using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using Pathfinding;

namespace BuildABot {

    /**
        A lot of help from this tutorial series by Blackthornprod

        https://www.youtube.com/watch?v=hk6cUanSfXQ
    */
    public class TutorialGenerator : LevelGenerator
    {
        
        void Awake()
        {
            tilemap.GetComponent<Tilemap>().ClearAllTiles();
        
            ChangeColor();

            // Let level generate then scan with A*
            Utility.DelayedFunction(this, 0.5f, () => {
                AstarPath.active.Scan();
            });

            generate = false;
        }
    }
}