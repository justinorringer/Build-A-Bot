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
    public class LevelGenerator : MonoBehaviour
    {
        /**
        * The direction the next room will be generated in.
        * 
        * Multiple left and right values to give the randomness weight
        * (want to go to the left or right more often than down)
        *
        * Right = 1, 2, Left = 3, 4, Down = 5
        */
        private enum Direction
        {
            Right1 = 1,
            Right2 = 2,
            Left1 = 3,
            Left2 = 4,
            Down = 5
        }

        private class Map {
            public class MapRoom {
                public bool isStart = false;
                public bool isEnd = false;

                public bool isLEnd = false; // Dead end with opening on the left
                public bool isREnd = false; // Dead end with opening on the right

                public bool isBrick = true; // unused Room when true

                public enum Connections {
                    Top,
                    Right,
                    Bottom,
                    Left
                }

                /**
                    index 0 --> Top, 1 --> Right, index 2--> Bottom, index 3 --> Left
                 */
                private bool[] connections = new bool[4] { false, false, false, false };

                public bool[] GetConnections() {
                    return connections;
                }

                /**
                    index 0 --> Top, 1 --> Right, index 2--> Bottom, index 3 --> Left
                 */
                public void Connect(Connections connection) {
                    int c = (int) connection;
                    connections[ c ] = true;

                    isBrick = false;
                }

                public RoomType GetRoomType() {
                    if (isStart) {
                        return RoomType.START;
                    } else if (isEnd) {
                        return RoomType.END;
                    } else if (isLEnd) {
                        return RoomType.LEND;
                    } else if (isREnd) {
                        return RoomType.REND;
                    } else if (connections[0] && connections[1] && connections[2] && connections[3]) {
                        return RoomType.TRBL;
                    } else if (connections[1] && connections[2] && connections[3]) {
                        return RoomType.RBL;
                    } else if (connections[0] && connections[2] && connections[3]) {
                        return RoomType.TRBL;
                    } else if (connections[0] && connections[1] && connections[3]) {
                        return RoomType.TRL;
                    } else if (connections[0] && connections[2]) {
                        return RoomType.TRBL;
                    } else if (connections[0] && connections[3]) {
                        return RoomType.TRL;
                    } else if (connections[1] && connections[2]) {
                        return RoomType.RBL;
                    } else if (connections[0]) {
                        return RoomType.TRL;
                    } else if (connections[1]) {
                        return RoomType.RL;
                    } else if (connections[2]) {
                        return RoomType.RBL;
                    } else if (connections[3]) {
                        return RoomType.RL;
                    } else {
                        return RoomType.NONE;
                    }
                }
            }

            /** 
                Currently supports mapSizes < 16
            */
            public int mapSize = 4;

            public MapRoom[ , ] grid = new MapRoom[ 8, 8 ];
        }

        private int[] startingPositions = new int[ ] {0,1,2,3};

        [SerializeField] public GameObject startRoom, endRoom, brickRoom, lEndRoom, rEndRoom;


        public GameObject[] roomTemplates; 
        // index 0 --> RL, index 1 --> RBL, index 2 --> TRL, index 3 --> TRBL

        public GameObject tilemap;

        [SerializeField] private Map map = new Map();
        
        /** downCounter prevents down twice, making lame levels */
        private int direction, downCounter = 0; // direction is the direction the next room will be generated in

        private int[] current = new int[] {0, 0};

        public int moveAmount = 8;

        public bool generate = true;

        void Start()
        {
            tilemap.GetComponent<Tilemap>().ClearAllTiles();

            int randStartingPos = Random.Range(0, startingPositions.Length);

            current[0] = startingPositions[randStartingPos];
            current[1] = 0;

            PopulateGrid();

            map.grid[current[0], current[1]].isStart = true;
            
            direction = Random.Range(1, 5);

            while (generate) {
                ChoosePath();
            }

            InstantiateGrid();

            // Let level generate then scan with A*
            Utility.DelayedFunction(this, 0.5f, () => {
                AstarPath.active.Scan();
            });
        }

        private void ChoosePath()
        {
            if (direction == 1 || direction == 2) { // Move RIGHT
                if (current[0] < map.mapSize - 1) {

                    downCounter = 0;

                    // give previous room a requirement for a right opening
                    map.grid[current[0], current[1]].Connect(Map.MapRoom.Connections.Right);

                    current[0] ++; // go to the room to the right

                    Map.MapRoom m = map.grid[current[0], current[1]]; // get the room to the right; [column, row]
                
                    m.Connect(Map.MapRoom.Connections.Left); // connect the room with left one

                    // next direction
                    direction = Random.Range(1, 6);
                    if (direction == 3) {
                        direction = 2;
                    } else if (direction == 4) {
                        direction = 5;
                    }
                } else {
                    if (map.grid[current[0], current[1]].isBrick || map.grid[current[0], current[1]].GetRoomType() == RoomType.START) {
                        direction = Random.Range(3, 5);

                        return;
                    }

                    direction = 5;
                }
            } else if (direction == 3 || direction == 4) { // Move LEFT
                if (current[0] == 0) {
                    direction = 5;
                } else {
                    downCounter = 0;

                    map.grid[current[0], current[1]].Connect(Map.MapRoom.Connections.Left);

                    current[0]--; // go to the room to the left

                    Map.MapRoom m = map.grid[current[0], current[1]]; // the room to the left; [column, row]

                    m.Connect(Map.MapRoom.Connections.Right); // connect the room to the right

                    direction = Random.Range(3, 6);
                }
            } else if (direction == 5) { // Move DOWN
                downCounter++;
                if (current[1] == map.mapSize - 1) {
                    map.grid[current[0], current[1]].isEnd = true;
                    generate = false;
                } else {
                    if (map.grid[current[0], current[1]].GetRoomType() == RoomType.START) { // just in case this happens somehow
                        direction = Random.Range(1, 5);

                        return;
                    }

                    map.grid[current[0], current[1]].Connect(Map.MapRoom.Connections.Bottom);

                    // Now, I want to create dead end rooms randomly. 
                    // First, I need to check if the left or right of this room is open
                    if (current[0] + 1 < map.mapSize - 1 && map.grid[current[0] + 1, current[1]].GetRoomType() == RoomType.NONE) {
                        // if so, create a dead end room
                        if (Random.Range(0, 3) == 0)
                            map.grid[current[0] + 1, current[1]].isLEnd = true; // set the room to be a dead end
                    } else if (current[0] - 1 >= 1 && map.grid[current[0] - 1, current[1]].GetRoomType() == RoomType.NONE) {
                        if (Random.Range(0, 3) == 0) // if so, create a dead end room
                            map.grid[current[0] - 1, current[1]].isREnd = true;
                    }

                    current[1] ++; // go to the room below
                    Map.MapRoom m = map.grid[current[0], current[1]]; // get the room below; [column, row]
                    m.Connect(Map.MapRoom.Connections.Top); // connect the room to the top


                    if (downCounter >= 1) { // going down too many times makes a bad map
                        downCounter = 0;
                        direction = Random.Range(1, 5);
                    } else {
                        direction = Random.Range(1, 6);
                    }
                }
            }
        }

        /**
         * Utility function to clear all tiles in an area
         */
        private void ClearAllTiles() {
            Utility.DelayedFunction(this, 5.0f, () => {
                tilemap.GetComponent<Tilemap>().ClearAllTiles();
            });
        }

        private void PopulateGrid() {
            for (int i = 0; i < map.mapSize; i++) {
                for (int j = 0; j < map.mapSize; j++) {
                    map.grid[i, j] = new Map.MapRoom();
                }
            }
        }
        private void InstantiateGrid() {

            for (int i = -1; i <= map.mapSize; i++) {
                for (int j = -1; j <= map.mapSize; j++) {

                    Vector3Int pos = new Vector3Int((int) i * moveAmount, (int) j * moveAmount * (-1), 0);

                    if (i == -1 || j == -1 || i == map.mapSize || j == map.mapSize) {
                        InstantiateRoom(brickRoom, pos);

                        continue;
                    } 

                    RoomType type = map.grid[i, j].GetRoomType(); // column is i, row is j

                    switch (type) {
                        case RoomType.RL:
                            InstantiateRoom(roomTemplates[0], pos);
                            break;
                        case RoomType.RBL:
                            InstantiateRoom(roomTemplates[1], pos);
                            break;
                        case RoomType.TRL:
                            InstantiateRoom(roomTemplates[2], pos);
                            break;
                        case RoomType.TRBL:
                            InstantiateRoom(roomTemplates[3], pos);
                            break;
                        case RoomType.START:
                            InstantiateRoom(startRoom, pos);
                            break;
                        case RoomType.END:
                            InstantiateRoom(endRoom, pos);
                            break;
                        case RoomType.LEND:
                            InstantiateRoom(lEndRoom, pos);
                            break;
                        case RoomType.REND:
                            InstantiateRoom(rEndRoom, pos);
                            break;
                        case RoomType.NONE:
                            InstantiateRoom(brickRoom, pos);
                            break;
                    }
                }
            }
        }

        private void InstantiateRoom(GameObject room, Vector3 position = new Vector3()) {

            GameObject r = (GameObject) Instantiate(room, position, Quaternion.identity);

            r.transform.parent = tilemap.transform;
        }
    }
}