using UnityEngine;

namespace BuildABot
{
    public class NPCSpawner : MonoBehaviour
    {
        [SerializeField] private GameObject npcToSpawn;

        [SerializeField] private int levelToSpawn;

        private void Awake()
        {
            if (GameManager.GameState.CompletedLevelCount == levelToSpawn)
            {
                //Debug.Log("Spawning NPC");
                Instantiate(npcToSpawn, transform.position, transform.rotation);
            }
        }
    }
}
