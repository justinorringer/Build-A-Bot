using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    public static class Utility
    {
        
        #region Gameplay Utilities

        /**
         * Quits the game with the provided exit code and termination message.
         * <param name="exitCode">The exit code of the application. This should be 0 for a successful execution.</param>
         * <param name="message">
         * A status message displayed if the exitCode represents an error. Null messages will be ignored.
         * </param>
         */
        public static void QuitGame(int exitCode = 0, string message = null)
        {
    #if UNITY_EDITOR
            EditorApplication.isPlaying = false;
            if (exitCode != 0)
            {
                if (message == null) Debug.LogWarningFormat("Play in editor exited with code {0}", exitCode);
                else Debug.LogWarningFormat("Play in editor exited with code {0}: {1}", exitCode, message);
            }
            else Debug.Log("Play in editor exited with code 0");
    #else
            Application.Quit(exitCode);
    #endif
        }
        
        #endregion

        #region Delayed Function Coroutine
        
        /**
         * Executes the provided action after the specified delay. This will be affected by the game's time scale if not using realtime.
         * The execution of this can be stopped early by calling StopCoroutine from the originally provided context
         * with the IEnumerator returned by this function.
         * <param name="context">The MonoBehaviour context used to handle the coroutine invocation.</param>
         * <param name="seconds">The number of seconds to wait before performing the action.</param>
         * <param name="action">The action to perform.</param>
         * <param name="realtime">Is this function performed in realtime, ignoring time scale? Defaults to false.</param>
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator DelayedFunction(MonoBehaviour context, float seconds, Action action, bool realtime = false)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot perform a null action.");
            IEnumerator coroutine = realtime ? DelayedFunctionImplementationRealtime(seconds, action) : DelayedFunctionImplementation(seconds, action);
            context.StartCoroutine(coroutine);
            return coroutine;
        }
        
        #region Delayed Function Coroutine Implementation

        /**
         * The underlying implementation used for delayed function calls.
         * <param name="seconds">The number of seconds to wait before performing the action.</param>
         * <param name="action">The action to perform.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator DelayedFunctionImplementation(float seconds, Action action)
        {
            yield return new WaitForSeconds(seconds);
            action();
        }

        /**
         * The underlying implementation used for delayed function calls in realtime.
         * <param name="seconds">The number of seconds to wait before performing the action.</param>
         * <param name="action">The action to perform.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator DelayedFunctionImplementationRealtime(float seconds, Action action)
        {
            yield return new WaitForSecondsRealtime(seconds);
            action();
        }
        
        #endregion
        
        #endregion
        
        #region Repeat Function Coroutines

        /**
         * Repeats the provided action indefinitely using the provided interval between calls.
         * The execution of this can only be stopped by calling StopCoroutine from the originally provided context
         * with the IEnumerator returned by this function.
         * <param name="context">The MonoBehaviour context used to handle the coroutine invocation.</param>
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="realtime">Is this function performed in realtime, ignoring time scale? Defaults to false.</param>
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator RepeatFunction(MonoBehaviour context, Action action, float interval, bool realtime = false)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot repeat a null action.");
            IEnumerator coroutine = realtime
                ? RepeatFunctionIndefiniteRealtimeImpl(action, interval)
                : RepeatFunctionIndefiniteImpl(action, interval);
            context.StartCoroutine(coroutine);
            return coroutine;
        }

        /**
         * Repeats the provided action the specified number of times using the provided interval between calls.
         * The execution of this can be stopped early by calling StopCoroutine from the originally provided context
         * with the IEnumerator returned by this function.
         * <param name="context">The MonoBehaviour context used to handle the coroutine invocation.</param>
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="count">The number of times to repeat the function.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed. Defaults to null.</param>
         * <param name="realtime">Is this function performed in realtime, ignoring time scale? Defaults to false.</param>
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator RepeatFunction(MonoBehaviour context, Action action, float interval, int count,
            Action onFinish = null, bool realtime = false)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot repeat a null action.");
            IEnumerator coroutine = realtime ?
                RepeatFunctionWithCountRealtimeImpl(action, interval, count, onFinish) :
                RepeatFunctionWithCountImpl(action, interval, count, onFinish);
            context.StartCoroutine(coroutine);
            return coroutine;
        }

        /**
         * Repeats the provided action until the condition function returns false while using the provided interval between calls.
         * The execution of this can be stopped by calling StopCoroutine from the originally provided context
         * with the IEnumerator returned by this function.
         * <param name="context">The MonoBehaviour context used to handle the coroutine invocation.</param>
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="condition">The function used to determine if the coroutine should continue to repeat. Returning true will continue.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed. Defaults to null.</param>
         * <param name="realtime">Is this function performed in realtime, ignoring time scale? Defaults to false.</param>
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator RepeatFunctionUntil(MonoBehaviour context, Action action, float interval, Func<bool> condition,
            Action onFinish = null, bool realtime = false)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot repeat a null action.");
            IEnumerator coroutine = realtime ?
                RepeatFunctionUntilRealtimeImpl(action, interval, condition, onFinish) :
                RepeatFunctionUntilImpl(action, interval, condition, onFinish);
            context.StartCoroutine(coroutine);
            return coroutine;
        }
        
        #region Repeat Function Coroutine Implementations

        /**
         * The internal coroutine implementation used to repeat a function at an interval indefinitely.
         * This can only exit execution by using MonoBehaviour.StopCoroutine(IEnumerator) from the original context.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionIndefiniteImpl(Action action, float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                action();
            }
        }

        /**
         * The internal coroutine implementation used to repeat a function at a realtime interval indefinitely.
         * this can only exit execution by using MonoBehaviour.StopCoroutine(IEnumerator) from the original context.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionIndefiniteRealtimeImpl(Action action, float interval)
        {
            while (true)
            {
                yield return new WaitForSecondsRealtime(interval);
                action();
            }
        }

        /**
         * The internal coroutine implementation used to repeat a function at an interval a specific number of times
         * this can exit execution early by using MonoBehaviour.StopCoroutine(IEnumerator) from the original context.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="count">The number of times to repeat the function.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionWithCountImpl(Action action, float interval, int count, Action onFinish)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(interval);
                action();
            }
            onFinish?.Invoke();
        }

        /**
         * The internal coroutine implementation used to repeat a function at a realtime interval a specific number of times
         * this can exit execution early by using MonoBehaviour.StopCoroutine(IEnumerator) from the original context.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="count">The number of times to repeat the function.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionWithCountRealtimeImpl(Action action, float interval, int count, Action onFinish)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSecondsRealtime(interval);
                action();
            }
            onFinish?.Invoke();
        }

        /**
         * The internal coroutine implementation used to repeat a function at an interval until the condition function returns true.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="condition">The function used to determine if the coroutine should continue to repeat. Returning true will continue.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionUntilImpl(Action action, float interval, Func<bool> condition, Action onFinish)
        {
            while (condition())
            {
                yield return new WaitForSeconds(interval);
                action();
            }
            onFinish?.Invoke();
        }

        /**
         * The internal coroutine implementation used to repeat a function at a realtime interval until the condition function returns true.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="condition">The function used to determine if the coroutine should continue to repeat. Returning true will continue.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionUntilRealtimeImpl(Action action, float interval, Func<bool> condition, Action onFinish)
        {
            while (condition())
            {
                yield return new WaitForSecondsRealtime(interval);
                action();
            }
            onFinish?.Invoke();
        }
        
        #endregion
        
        #endregion

        #region String Handling
        
        /**
         * Replaces any instance of the key values of the provided token dictionary in this string with the key's
         * corresponding value.
         * <remarks>This is an extension method for the string type.</remarks>
         * <param name="source">The string to replace the tokens of.</param>
         * <param name="tokens">The tokens to replace mapped to the replacement value.</param>
         * <returns>The updated string value with all tokens replaced.</returns>
         */
        public static string ReplaceTokens(this string source, Dictionary<string, string> tokens)
        {
            string result = source;
            foreach (var entry in tokens)
            {
                result = result.Replace(entry.Key, entry.Value);
            }
            return result;
        }

        /**
         * Returns a reader friendly version of the provided variable name.
         */
        public static string NicifyVariableName(string name)
        {
            if (name.Length <= 1) return name.ToUpper();
            
            char[] data = name.ToCharArray();
            int start = 0;
            StringBuilder result = new StringBuilder();
            
            // Trim special prefixes
            if (data.Length > 1 && data[0] == '-') start++; // Starts with _
            else if (data.Length > 2 && data[0] == 'm' && data[1] == '_') start += 2; // Starts with m_
            else if (data.Length > 1 && data[0] == 'k' && data[1] >= 'A' && data[1] <= 'Z') start++; // Starts with k[A-Z]
            
            // Skip all other leading underscores
            while (start < data.Length && data[start] == '_') start++;

            if (start >= data.Length) return "";
            
            // Capitalize the first character
            if (data[start].IsLower()) data[start] = data[start].ToUpper();

            for (int i = start; i < data.Length; i++)
            {
                if (data[i] == '_')
                {
                    // Replace underscores with spaces
                    if (i != start && data[i - 1] != '_') result.Append(' '); // Skip multiple underscores
                }
                else if (i > start && data[i - 1] == '_' && data[i].IsLower())
                {
                    // Capitalize lower case letters that follow _
                    result.Append(data[i].ToUpper());
                }
                else if (i > start && data[i].IsUpper() && !data[i - 1].IsUpper() && data[i - 1] != '_')
                {
                    // Insert space before new sequences of capital letters
                    // Excludes following _ b/c that should be handled already
                    result.Append(' ');
                    result.Append(data[i]);
                }
                else if (i > start && data[i].IsNumber() && !data[i - 1].IsNumber() && data[i - 1] != '_')
                {
                    // Insert space before new sequences of numbers
                    // Excludes following _ b/c that should be handled already
                    result.Append(' ');
                    result.Append(data[i]);
                }
                else if (i > start && data[i].IsLower() && data[i - 1].IsNumber())
                {
                    // Insert space before word after number and capitalize
                    result.Append(' ');
                    result.Append(data[i].ToUpper());
                }
                else result.Append(data[i]);
            }
            
            return result.ToString();
        }
        
        #endregion
    }
}
