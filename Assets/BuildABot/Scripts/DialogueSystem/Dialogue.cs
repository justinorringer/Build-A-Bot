using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace BuildABot
{
    [CreateAssetMenu(fileName = "Dialogue", menuName = "Build-A-Bot/Dialogue/Dialogue", order = 1)]
    public class Dialogue : ScriptableObject
    {
        public DialogueSpeaker character;

        [TextArea(3, 10)]
        public string[] sentences;
    }
}