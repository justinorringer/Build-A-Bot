using UnityEngine;

namespace BuildABot
{
    public class HUD : MonoBehaviour
    {
        [Tooltip("The player instance using this HUD.")]
        [SerializeField] private Player player;
    }
}
