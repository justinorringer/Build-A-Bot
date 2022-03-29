using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BuildABot
{
    [CreateAssetMenu(fileName = "DialogueSpeaker", menuName = "Build-A-Bot/Dialogue/DialogueSpeaker", order = 1)]
    public class DialogueSpeaker : ScriptableObject
    {
        public Sprite characterImage;

        public AudioClip speakingSound;

        public string characterName;
    }
}