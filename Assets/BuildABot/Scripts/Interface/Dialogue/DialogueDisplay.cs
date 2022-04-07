using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace BuildABot
{
    
    public class DialogueDisplay : MonoBehaviour
    {
        [Header("References")]
        
        [Tooltip("The player that is displaying this dialogue window.")]
        [SerializeField] private Player player;
        
        [Tooltip("Reference to the UI element where the dialogue speaker's image should be displayed")]
        [SerializeField] private Image characterImage;
        
        [Tooltip("Reference to the UI element where the dialogue text should be displayed")]
        [SerializeField] private TextMeshProUGUI dialogueText;
        
        [Tooltip("Reference to the UI element where the dialogue speaker's name should be displayed")]
        [SerializeField] private TextMeshProUGUI nameText;
        
        [Tooltip("Reference to the animator which moves the dialogue UI element in and out of frame")]
        [SerializeField] private Animator animator;

        [Tooltip("The audio source component used to play the sounds for this dialogue.")]
        [SerializeField] private AudioSource audioSource;

        [Tooltip("The prefab to instance for each response.")]
        [SerializeField] private DialogueResponseWidget responsePrefab;

        [Tooltip("The root of the response options list in the scene.")]
        [SerializeField] private GameObject responseOptionsRoot;
        
        [Header("Events")]
        
        [Tooltip("An event triggered when the current dialogue display is first opened.")]
        [SerializeField] private UnityEvent<Dialogue, DialogueSpeaker> onBeginDialogue;
        [Tooltip("An event triggered when the current dialogue display is finished and closed.")]
        [SerializeField] private UnityEvent<Dialogue, DialogueSpeaker> onEndDialogue;
        
        //TODO: Add more events for Dialogue UI
        
        /** An event triggered when the current dialogue display is finished and closed. */
        public event UnityAction<Dialogue, DialogueSpeaker> OnBeginDialogue
        {
            add => onBeginDialogue.AddListener(value);
            remove => onBeginDialogue.RemoveListener(value);
        }
        
        /** An event triggered when the current dialogue display is finished and closed. */
        public event UnityAction<Dialogue, DialogueSpeaker> OnEndDialogue
        {
            add => onEndDialogue.AddListener(value);
            remove => onEndDialogue.RemoveListener(value);
        }

        [Header("Settings")]
        
        [Tooltip("The amount of time in seconds it takes to type out each character.")]
        [SerializeField] private float characterTypingSpeed = 0.05f;

        /** The current dialogue tree being displayed. */
        private Dialogue _currentlyPlaying;
        /** The current dialogue speaker profile. */
        private DialogueSpeaker _currentlySpeaking;
        /** The current dialogue node. */
        private DialogueNode _currentNode;

        /** The coroutine enumerator for displaying the current node text. */
        private IEnumerator _displayTextCoroutine;
        /** The coroutine enumerator for finalizing the current dialogue display. */
        private IEnumerator _finalizeDisplayCoroutine;

        /** The instanced response options. */
        private List<DialogueResponseWidget> _responseOptions;

        /** Private flag to know if dialogue is currently being played */
        private bool _playing;

        /** Is dialogue currently being typed to the screen? */
        private bool _isTyping;
        /** Should the dialogue advance to the next selection? */
        private bool _shouldContinue;

        /** Is the finalization process waiting on a response. */
        private bool _isWaitingForResponse;
        /** The selected response option. */
        private int _selectedOption;

        /** The pre-hashed value for the IsOpen animator key. */
        private int _isOpenAnimatorKey;

        protected void Awake()
        {
            _isOpenAnimatorKey = Animator.StringToHash("IsOpen");
        }

        protected void OnEnable()
        {
            player.PlayerController.InputActions.DialogueUI.Continue.performed += Input_OnContinue;
        }

        protected void OnDisable()
        {
            player.PlayerController.InputActions.DialogueUI.Continue.performed -= Input_OnContinue;
        }

        private void UpdateSpeakerDisplay(DialogueSpeaker speaker, int expression = -1)
        {
            _currentlySpeaking = speaker;
            if (speaker != null)
            {
                nameText.text = speaker.CharacterName;

                // Try to use expression data
                if (expression >= 0 && expression < speaker.Expressions.Count)
                {
                    // If override exists, use it
                    characterImage.sprite = speaker.Expressions[expression].SpriteOverride == null ? speaker.DefaultCharacterSprite :
                        speaker.Expressions[expression].SpriteOverride;
                    audioSource.clip = speaker.Expressions[expression].SoundOverride == null ? speaker.SpeakingSound :
                        speaker.Expressions[expression].SoundOverride;
                }
                else
                {
                    // Use default
                    characterImage.sprite = speaker.DefaultCharacterSprite;
                    audioSource.clip = speaker.SpeakingSound;
                }
                
                // Handle the sprite along with any expression overrides
            }
            else
            {
                // TODO: Hide the character information, expand the text area
                nameText.text = "???";
                characterImage.sprite = null; // TODO: Create default sprite and sound
                audioSource.clip = null;
            }
        }

        /**
         * Attempts to start displaying the provided dialogue tree with the specified speaker.
         * <param name="dialogue">The dialogue tree to play.</param>
         * <param name="speaker">The speaker profile to display with the dialogue.</param>
         * <returns>True if dialogue could be successfully started.</returns>
         */
        public bool TryStartDialogue(Dialogue dialogue, DialogueSpeaker speaker)
        {
            
            gameObject.SetActive(true);

            // Handle empty dialogue tree
            if (dialogue == null || dialogue.DialogueNodes.Count == 0) return false;

            if (_currentlyPlaying != null) return false; // Still displaying a dialogue tree

            _currentlyPlaying = dialogue;
            
            // Update status
            //animator.SetBool(_isOpenAnimatorKey, true);
            _playing = true;

            _shouldContinue = false;
            _isTyping = false;
            _selectedOption = -1;
            _isWaitingForResponse = false;
            
            _displayTextCoroutine = null;
            _finalizeDisplayCoroutine = null;

            // Update UI and sound
            UpdateSpeakerDisplay(speaker);

            player.PlayerController.InputActions.Player.Disable();
            player.PlayerController.InputActions.DialogueUI.Enable();
            
            onBeginDialogue.Invoke(dialogue, speaker);

            //Start displaying sentences
            DisplayDialogueNode(dialogue.DialogueNodes[0]);

            return true;
        }

        public void EndDialogue()
        {
            if (_displayTextCoroutine != null)
            {
                StopCoroutine(_displayTextCoroutine);
                _displayTextCoroutine = null;
            }
            
            player.PlayerController.InputActions.DialogueUI.Disable();
            player.PlayerController.InputActions.Player.Enable();
            
            onEndDialogue.Invoke(_currentlyPlaying, _currentlySpeaking);
            
            _playing = false;
            //animator.SetBool(_isOpenAnimatorKey, false);
            dialogueText.text = "";
            _currentlyPlaying = null;
            UpdateSpeakerDisplay(null);
            // Should wait for animator?
            gameObject.SetActive(false);
        }

        private void DisplayDialogueNode(DialogueNode node)
        {
            _currentNode = node;
            
            // Update the expression
            UpdateSpeakerDisplay(_currentlySpeaking, node.ExpressionOverride);

            // Clear existing content
            dialogueText.text = "";
            
            // Display the content of this node
            
            if (_displayTextCoroutine != null) StopCoroutine(_displayTextCoroutine);
            
            _displayTextCoroutine = DisplayNodeImplementation(node);
            StartCoroutine(_displayTextCoroutine);
        }

        private void TryAdvanceDialogue()
        {
            if (!_playing) return;
            // Either cancel the typing coroutine and force fill the content or move to next node
            if (_isTyping && _displayTextCoroutine != null && _finalizeDisplayCoroutine == null)
            {
                StopCoroutine(_displayTextCoroutine);
                audioSource.Stop();
                _displayTextCoroutine = null;
                dialogueText.text = _currentNode.Content;
                _finalizeDisplayCoroutine = FinalizeNodeDisplay(_currentNode);
                StartCoroutine(_finalizeDisplayCoroutine);
            }
            else
            {
                _shouldContinue = true;
            }
        }

        public void SelectResponse(int option)
        {
            // Check if waiting for response, early exit
            if (!_isWaitingForResponse) return;
            // Check if option is valid
            if (option < 0 || option >= _currentNode.ResponseOptions.Count) return;
            // Cache option
            _selectedOption = option;
            // Set waiting to false
            _isWaitingForResponse = false;
        }

        private IEnumerator DisplayNodeImplementation(DialogueNode node)
        {
            
            // TODO: Handle standardized token replacement
            _isTyping = true;
            dialogueText.text = "";
            foreach (char letter in node.Content)
            {
                audioSource.Play();
                dialogueText.text += letter;

                yield return new WaitForSecondsRealtime(characterTypingSpeed); // Wait for delay
                audioSource.Stop();
            }
            _finalizeDisplayCoroutine = FinalizeNodeDisplay(node);
            StartCoroutine(_finalizeDisplayCoroutine);
        }

        private IEnumerator FinalizeNodeDisplay(DialogueNode node)
        {
            _isTyping = false;
            int next = node.NextNode;

            if (node.ResponseOptions.Count > 0)
            {
                _isWaitingForResponse = true;
                
                // Display the response option ui
                responseOptionsRoot.gameObject.SetActive(true);
                _responseOptions = new List<DialogueResponseWidget>();
                Cursor.visible = true;
                int i = 0;
                foreach (DialogueResponse response in node.ResponseOptions)
                {
                    DialogueResponseWidget widget = Instantiate(responsePrefab, responseOptionsRoot.transform);
                    widget.Initialize(this, i, response);
                    _responseOptions.Add(widget);
                    i++;
                }
                
                _responseOptions[0].GetComponent<Button>().Select();
                
                // Wait until a selection has been received
                yield return new WaitUntil(() => !_isWaitingForResponse);
                next = node.ResponseOptions[_selectedOption].TargetNode;
                
                // Hide and clear the response ui
                responseOptionsRoot.gameObject.SetActive(false);
                foreach (DialogueResponseWidget widget in _responseOptions)
                {
                    Destroy(widget.gameObject);
                }
                _responseOptions.Clear();
                Cursor.visible = false;
            }
            else
            {
                // Wait for continue option
                _shouldContinue = false;
                yield return new WaitUntil(() => _shouldContinue);
                _shouldContinue = false;
            }
            
            // Dispatch events from the current node
            foreach (GameplayEvent e in node.CompletionEvents)
            {
                e.Invoke();
            }

            _finalizeDisplayCoroutine = null;
            
            if (next == -1 || next >= _currentlyPlaying.DialogueNodes.Count) EndDialogue();
            else DisplayDialogueNode(_currentlyPlaying.DialogueNodes[next]);
        }
        
        
        private void Input_OnContinue(InputAction.CallbackContext context)
        {
            TryAdvanceDialogue();
        }
    }
}
