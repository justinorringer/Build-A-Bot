using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.Tilemaps;

namespace BuildABot {
    public class Spawn : MonoBehaviour
    {
        [SerializeField] public LootTable spawns;

        [SerializeField] private ItemPickup itemPickup;

        [SerializeField] private int xPos = 4;
        [SerializeField] private int yPos = 4;

        void Start()
        {
            int x = (int) transform.position.x + xPos;
            int y = (int) transform.position.y + yPos;

            // Make a vector 3 to store the position of the object .SetTile is particular
            Vector3Int pos = new Vector3Int(x, y, 0);

            InventoryEntry i = spawns.GenerateItemList()[0];
            ItemPickup pickup = Instantiate(itemPickup, pos, Quaternion.identity);

            pickup.Item = i.Item;

            pickup.Count = i.Count;

            //pickup.tooltipMessage = i.Item.TooltipMessage;
        }
    }
}