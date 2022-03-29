using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;

namespace BuildABot
{
    public class DialogueManager : MonoBehaviour
    {
        [Header("UI Element References")]
        [Tooltip("Reference to the UI element where the dialogue speaker's image should be displayed")]
        [SerializeField] private Image characterImage;
        [Tooltip("Reference to the UI element where the dialogue text should be displayed")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        [Tooltip("Reference to the UI element where the dialogue speaker's name should be displayed")]
        [SerializeField] private TextMeshProUGUI nameText;
        [Tooltip("Reference to the animator which moves the dialogue UI element in and out of frame")]
        [SerializeField] private Animator animator;

        /** Queue to store sentences in the order they should be read */
        private Queue<string> _sentences;

        /** Private reference to the audio player for dialogue sounds */
        private AudioSource _audioPlayer;

        /** Private flag to know if dialogue is currently being played */
        bool _playing;

        /** Singleton instance */
        private static DialogueManager _instance;

        public static DialogueManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    GameObject instance = new GameObject("DialogueManager");
                    instance.AddComponent<DialogueManager>();
                }

                return _instance;
            }
        }

        void Awake()
        {
            _instance = this;
        }

        void Start()
        {
            //Initialize fields
            _sentences = new Queue<string>();
            _audioPlayer = GetComponent<AudioSource>();
        }

        void Update()
        {
            //Check for user input
            if (Input.GetKeyUp(KeyCode.Space) && _playing)
            {
                Debug.Log("KeyUp");
                DisplayNextSentence();
            }
        }

        public void StartDialogue(Dialogue dialogue)
        {
            //Update status
            animator.SetBool("IsOpen", true);
            _playing = true;

            //Update UI and sound
            characterImage.sprite = dialogue.character.characterImage;
            _audioPlayer.clip = dialogue.character.speakingSound;
            nameText.text = dialogue.character.characterName;

            //Prep to display sentences
            _sentences.Clear();
            foreach (string sentence in dialogue.sentences)
            {
                _sentences.Enqueue(sentence);
            }

            //Start displaying sentences
            DisplayNextSentence();
        }

        public void EndDialogue()
        {
            _playing = false;
            animator.SetBool("IsOpen", false);
        }

        public void DisplayNextSentence()
        {
            //Check to see if we're out of sentences
            if (_sentences.Count == 0 && _playing)
            {
                //If so, end this dialogue
                EndDialogue();
                return;
            }

            //Otherwise, continue displaying sentences
            string sentence = _sentences.Dequeue();

            //TODO Replace this coroutine sloppiness with a better method from Utilities
            StopAllCoroutines(); //Allows for skipping dialogue
            StartCoroutine(TypeSentence(sentence));
        }

        IEnumerator TypeSentence(string sentence)
        {
            //Bit of a messy routine, but it gives the nice visual and sound effect
            dialogueText.text = "";
            foreach (char letter in sentence.ToCharArray())
            {
                _audioPlayer.Play();
                dialogueText.text += letter;

                yield return new WaitForSeconds(0.05f); //Wait a single frame
                _audioPlayer.Stop();
            }
        }
    }
}
