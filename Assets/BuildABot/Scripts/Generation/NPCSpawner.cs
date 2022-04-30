using UnityEngine;

namespace BuildABot
{
    public class NPCSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject npcToSpawn;

        [SerializeField] private int levelToSpawn;

        private void Start()
        {
            if (GameManager.GameState.CompletedLevelCount == levelToSpawn - 1)
            {
                Debug.Log("Spawning NPC");
                Instantiate(npcToSpawn, transform.position, transform.rotation);
            }
        }
    }
}
