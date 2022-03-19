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
        
        /**
         * The properties and data associated with a specific command.
         */
        private struct CommandProperties
        {

            /** A function that can be used to validate input arguments. */
            public delegate bool ValidateArgsFunc(string[] args);
            
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
            Debug.LogFormat("<b><i>{0}</i></b>\n    {1}\n    <i>Usage:</i> {2}\n", command, properties.Description, properties.Usage);
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
                    Action = (console, args) => {}
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
                    Action = (console, args) => {}
                }
            },
            { // Print player active effects
                "player.setStat", new CommandProperties {
                    Description = "prints all of the player's active effects to the console",
                    Usage = "player.setStat {attributeName} {value}",
                    ValidateArgs = args => ExpectArgCount(args, 2),
                    Action = (console, args) => {}
                }
            }
        };

        public void OnEnable()
        {
            inputField.onSubmit.AddListener(ExecuteInput);
            Application.logMessageReceived += HandleMessage;
        }

        public void OnDisable()
        {
            inputField.onSubmit.RemoveListener(ExecuteInput);
            Application.logMessageReceived -= HandleMessage;
        }

        private void HandleMessage(string message, string trace, LogType type)
        {
            switch (type)
            {
                case LogType.Log:
                    consoleText.text += $"{message}\n";
                    break;
                case LogType.Warning:
                    consoleText.text += $"Warning: <color=yellow>{message}</color>\n";
                    break;
                case LogType.Error:
                    consoleText.text += $"Error: <color=red>{message}</color>\n";
                    break;
                case LogType.Exception:
                    consoleText.text += $"Exception: <color=red>{message}</color>\n";
                    break;
                case LogType.Assert:
                    consoleText.text += $"Assert: <color=red>{message}</color>\n";
                    break;
            }
        }

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

            // TODO: store previous commands in list, access with up and down keys
            ClearInput();
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
            inputField.Select();
        }

    }
}
