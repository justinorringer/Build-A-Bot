using TMPro;
using UnityEngine;

namespace BuildABot
{
    public class DialogueResponseWidget : MonoBehaviour
    {

        [SerializeField] private TMP_Text text;
        
        private DialogueDisplay Display { get; set; }
        private DialogueResponse Response { get; set; }
        
        private int Index { get; set; }

        public void Initialize(DialogueDisplay display, int index, DialogueResponse response)
        {
            Display = display;
            Response = response;
            Index = index;
            text.text = response.Text;
        }

        public void Select()
        {
            Display.SelectResponse(Index);
        }
    }
}