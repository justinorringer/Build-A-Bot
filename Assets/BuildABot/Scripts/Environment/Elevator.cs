using System;
using UnityEngine;

namespace BuildABot
{
    public class Elevator : MonoBehaviour
    {
        private void OnTriggerEnter2D(Collider2D other)
        {
            Player player = other.gameObject.GetComponent<Player>();
            if (player != null)
            {
                player.FinishGame("You Win!");
            }
        }
    }
}