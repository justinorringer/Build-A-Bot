using UnityEngine;

namespace BuildABot
{
    public class DestroyObjectControl : MonoBehaviour, IMenuControl
    {

        [Tooltip("The game object to target and destroy when this control executes.")]
        [SerializeField] private GameObject target;
        
        public void Execute()
        {
            Destroy(target);
        }
    }
}