using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace BuildABot
{
    /**
     * Provides command line functionality as a MonoBehaviour.
     */
    public sealed class CommandConsole : MonoBehaviour
    {

        [Tooltip("A reference to the debug display that owns this console.")]
        [SerializeField] private DebugDisplay debugDisplay;

        [Tooltip("A reference to the current player object.")]
        [SerializeField] private Player player;

        [Header("UI")]
        
        [Tooltip("The input field used for this command line.")]
        [SerializeField] private TMP_InputField inputField;

        [Tooltip("The text area used by the console.")]
        [SerializeField] private TMP_Text consoleText;

        /** The cache of entries that have been used for the command line during the current program execution. */
        private readonly List<string> _entries = new List<string>();

        /** The index of the current input entry. */
        private int _activeEntryIndex;
        /** A cache of the latest entry value (not submitted) used when selecting previous inputs. */
        private string _latestInputCache = "";
        

        /** A function that can be used to validate input arguments. */
        public delegate bool ValidateArgsFunc(string[] args);
        
        /**
         * The properties and data associated with a specific command.
         */
        private struct CommandProperties
        {
            
            /** The description of this command displayed by the help option. */
            public string Description;
            /** The usage format of the command. */
            public string Usage;

            /** The validation function used to verify the arguments for this command. */
            public ValidateArgsFunc ValidateArgs;
            
            /** The function executed by the command. Takes in an array of arguments including the command itself. */
            public Action<CommandConsole, string[]> Action;
        }

        /**
         * Checks that the arg array provided has a number of arguments that match one of the provided options.
         * <param name="args">The command line args including the command itself.</param>
         * <param name="expected">The required default argument count.</param>
         * <param name="options">Alternative argument count options.</param>
         * <returns>True if the argument array is valid in terms of argument count.</returns>
         */
        private static bool ExpectArgCount(string[] args, int expected, params int[] options)
        {
            int count = args.Length - 1;
            if (expected == count) return true; // Default option is used
            foreach (int option in options)
                if (option == count)
                    return true; // Alternate option is used
            return false; // No matching option found
        }

        /**
         * Prints the help information for the provided command to the console if valid.
         * <param name="command">The command to get help for.</param>
         */
        private static void PrintCommandHelp(string command)
        {
            if (Commands.TryGetValue(command, out CommandProperties properties))
            {
                PrintCommandHelp(command, properties);
            }
            else Debug.LogErrorFormat("Unknown command '{0}'", command);
        }

        /**
         * Prints the help information for the provided command to the console if valid.
         * <param name="command">The command to get help for.</param>
         * <param name="properties">The properties associated with the command.</param>
         */
        private static void PrintCommandHelp(string command, CommandProperties properties)
        {
            Debug.LogFormat("<b><i>{0}</i></b>\n    {1}\n    <i>Usage:</i> {2}", command, properties.Description, properties.Usage);
        }
        
        /** The dictionary of all commands available to the game by default. */
        private static readonly Dictionary<string, CommandProperties> Commands = new Dictionary<string, CommandProperties>
        {
            { // Command line help
                "help", new CommandProperties {
                    Description = "prints information about every available command to the command console, or about a specific command if one is provided as an argument",
                    Usage = "help [command]",
                    ValidateArgs = args => ExpectArgCount(args, 0, 1),
                    Action = (console, args) =>
                    {
                        // Note that the argument count is validated before call to action
                        
                        // Version with no args
                        if (args.Length == 1)
                        {
                            foreach (var entry in Commands)
                            {
                                PrintCommandHelp(entry.Key, entry.Value);
                            }
                        }
                        // Version with command arg
                        else PrintCommandHelp(args[1]);
                    }
                    
                }
            },
            { // Quit Game
                "quit", new CommandProperties {
                    Description = "quits the game",
                    Usage = "quit",
                    ValidateArgs = args => ExpectArgCount(args, 0),
                    Action = (console, args) => Utility.QuitGame(0, "Quit game called from command line")
                }
            },
            { // Toggle FPS display
                "fps", new CommandProperties {
                    Description = "toggles the live fps display on screen",
                    Usage = "fps",
                    ValidateArgs = args => ExpectArgCount(args, 0),
                    Action = (console, args) => console.debugDisplay.ToggleFPS()
                }
            },
            { // Print player stats
                "player.stats", new CommandProperties {
                    Description = "prints all of the player's stats and attributes to the console",
                    Usage = "player.stats",
                    ValidateArgs = args => ExpectArgCount(args, 0),
                    Action = (console, args) => { Debug.Log(console.player.Attributes.ToString()); }
                }
            },
            { // Print player active effects
                "player.effects", new CommandProperties {
                    Description = "prints all of the player's active effects to the console",
                    Usage = "player.effects",
                    ValidateArgs = args => ExpectArgCount(args, 0),
                    Action = (console, args) =>
                    {
                        Debug.LogWarning("player.effects has not yet been implemented");
                        //console.player.Attributes.GetActiveEffects();
                    }
                }
            },
            { // Print player active effects
                "player.setStat", new CommandProperties {
                    Description = "sets the specified player attribute to the provided value",
                    Usage = "player.setStat {attributeName} {value}",
                    ValidateArgs = args => ExpectArgCount(args, 2),
                    Action = (console, args) =>
                    {
                        AttributeSet playerAttributes = console.player.Attributes;
                        AttributeDataBase attribute = playerAttributes.GetAttributeData(args[1]);
                        if (attribute.DataType == typeof(float))
                        {
                            ((AttributeData<float>)attribute).BaseValue = float.Parse(args[2]);
                        }
                        else
                        {
                            ((AttributeData<int>)attribute).BaseValue = int.Parse(args[2]);
                        }
                    }
                }
            },
            {
                "fly", new CommandProperties {
                    Description = "toggles fly mode for the player",
                    Usage = "fly",
                    ValidateArgs = args => ExpectArgCount(args, 0),
                    Action = (console, args) =>
                    {
                        ECharacterMovementMode mode = console.player.PlayerMovement.MovementMode;
                        console.player.PlayerMovement.ChangeMovementMode(mode == ECharacterMovementMode.Flying ?
                            ECharacterMovementMode.Walking : ECharacterMovementMode.Flying);
                        Debug.LogFormat("Fly mode {0}", console.player.PlayerMovement.IsFlying ? "enabled" : "disabled");
                    }
                }
            }
        };

        /**
         * Registers a command to the global command registry at runtime. Calling this function with an already registered
         * command name will overwrite the existing command's behavior.
         * <param name="command">The command string to register.</param>
         * <param name="description">The description of the command displayed when getting help for the command.</param>
         * <param name="usage">A string displayed in help menus to show the format expected for the command.</param>
         * <param name="action">The action executed by the command when run. Takes a reference to the command console instance and the string arguments.</param>
         * <param name="expectedArgCount">The number of arguments expected for the command by default during input validation.</param>
         * <param name="argCountOptions">Alternative argument count options used for input validation.</param>
         */
        public void RegisterRuntimeCommand(string command, string description, string usage,
            Action<CommandConsole, string[]> action, int expectedArgCount, params int[] argCountOptions)
        {
            RegisterRuntimeCommand(command, new CommandProperties
            {
                Description = description, Usage = usage,
                ValidateArgs = args => ExpectArgCount(args, expectedArgCount, argCountOptions),
                Action = action
            });
        }

        /**
         * Registers a command to the global command registry at runtime. Calling this function with an already registered
         * command name will overwrite the existing command's behavior.
         * <param name="command">The command string to register.</param>
         * <param name="description">The description of the command displayed when getting help for the command.</param>
         * <param name="usage">A string displayed in help menus to show the format expected for the command.</param>
         * <param name="action">The action executed by the command when run. Takes a reference to the command console instance and the string arguments.</param>
         * <param name="argValidationFunc">The validation function used to verify that the entered command arguments are valid.</param>
         */
        public void RegisterRuntimeCommand(string command, string description, string usage,
            Action<CommandConsole, string[]> action, ValidateArgsFunc argValidationFunc)
        {
            RegisterRuntimeCommand(command, new CommandProperties
            {
                Description = description, Usage = usage, ValidateArgs = argValidationFunc, Action = action
            });
        }

        /**
         * Registers a command to the global command registry at runtime.
         * <param name="command">The command string to register.</param>
         * <param name="properties">The properties of the command.</param>
         */
        private void RegisterRuntimeCommand(string command, CommandProperties properties)
        {
            Commands.Add(command, properties);
        }

        public void OnEnable()
        {
            inputField.onSubmit.AddListener(ExecuteInput);
            Application.logMessageReceived += HandleMessage;
            player.PlayerInput.InputEnabled = false;
        }

        public void OnDisable()
        {
            inputField.onSubmit.RemoveListener(ExecuteInput);
            Application.logMessageReceived -= HandleMessage;
            player.PlayerInput.InputEnabled = true;
        }

        /**
         * Handles printing debug messages to the console.
         * <param name="message">The message to display.</param>
         * <param name="trace">The associated stack trace.</param>
         * <param name="type">The logging type used for the message.</param>
         */
        private void HandleMessage(string message, string trace, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    consoleText.text += $"{message}\n";
                    break;
                case LogType.Warning:
                    consoleText.text += $"<color=yellow>Warning: {message}</color>\n";
                    break;
                case LogType.Error:
                    consoleText.text += $"<color=red>Error: {message}</color>\n";
                    break;
                case LogType.Exception:
                    consoleText.text += $"<color=red>Exception: {message}</color>\n";
                    break;
                case LogType.Assert:
                    consoleText.text += $"<color=red>Assert: {message}</color>\n";
                    break;
            }
        }

        /**
         * Executes the provided command input if it is valid.
         * <param name="value">The input value to process.</param>
         */
        private void ExecuteInput(string value)
        {
            // Skip empty inputs
            if (string.IsNullOrWhiteSpace(value)) return;
            // Get the command any any provided arguments
            string[] args = value.Split(' ');
            string command = args[0];
            if (Commands.TryGetValue(command, out CommandProperties data))
            {
                // Ensure that the command has valid arguments
                if (data.ValidateArgs(args)) data.Action(this, args); // If valid, execute
                else Debug.LogErrorFormat("Invalid arguments - Usage: {0}", data.Usage);
            }
            else Debug.LogErrorFormat("Invalid command: '{0}'", value);

            _entries.Add(value);
            _activeEntryIndex = _entries.Count;
            
            ClearInput();
            Focus();
        }

        /**
         * Clears the input text field of the console.
         */
        public void ClearInput()
        {
            inputField.text = "";
        }

        /**
         * Focuses control onto the input field.
         */
        public void Focus()
        {
            inputField.OnSelect(null);
        }

        private void Update()
        {
            // Handle moving up and down through old inputs
            if (inputField.isFocused && _entries.Count > 0)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    if (_activeEntryIndex == _entries.Count) _latestInputCache = inputField.text;
                    _activeEntryIndex--;
                    if (_activeEntryIndex < 0) _activeEntryIndex = 0;
                    inputField.text = _entries[_activeEntryIndex];
                    inputField.caretPosition = inputField.text.Length;
                }

                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    _activeEntryIndex++;
                    if (_activeEntryIndex > _entries.Count) _activeEntryIndex = _entries.Count;
                    else
                    {
                        inputField.text = _activeEntryIndex == _entries.Count ?
                            _latestInputCache : _entries[_activeEntryIndex];
                        inputField.caretPosition = inputField.text.Length;
                    }
                }
            }
        }
    }
}
