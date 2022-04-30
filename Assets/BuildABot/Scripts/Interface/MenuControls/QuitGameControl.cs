using UnityEngine;

namespace BuildABot
{
    public class QuitGameControl : MonoBehaviour, IMenuControl
    {
        public void Execute()
        {
            Utility.QuitGame();
        }
    }
}