using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace BuildABot
{
    public static class Utility
    {
        /**
         * Quits the game with an exit code of 0 and no status message.
         */
        public static void QuitGame()
        {
            QuitGame(0, null);
        }

        /**
         * Quits the game with the provided exit code and termination message.
         * <param name="exitCode">The exit code of the application. This should be 0 for a successful execution.</param>
         * <param name="message">
         * A status message displayed if the exitCode represents an error. Null messages will be ignored.
         * </param>
         */
        public static void QuitGame(int exitCode, string message = null)
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

        /**
         * Executes the provided action after the specified delay.
         * The execution of this can be stopped early by calling StopCoroutine from the originally provided context
         * with the IEnumerator returned by this function.
         * <param name="context">The MonoBehaviour context used to handle the coroutine invocation.</param>
         * <param name="seconds">The number of seconds to wait before performing the action.</param>
         * <param name="action">The action to perform.</param>
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator DelayedFunction(MonoBehaviour context, float seconds, Action action)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot perform a null action.");
            IEnumerator coroutine = DelayedFunctionImplementation(seconds, action);
            context.StartCoroutine(coroutine);
            return coroutine;
        }

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
         * Repeats the provided action indefinitely using the provided interval between calls.
         * The execution of this can only be stopped by calling StopCoroutine from the originally provided context
         * with the IEnumerator returned by this function.
         * <param name="context">The MonoBehaviour context used to handle the coroutine invocation.</param>
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator RepeatFunction(MonoBehaviour context, Action action, float interval)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot repeat a null action.");
            IEnumerator coroutine = RepeatFunctionIndefiniteImplementation(action, interval);
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
         * <returns>The coroutine enumerator.</returns>
         * <exception cref="ArgumentNullException">Thrown if the provided action or context is null.</exception>
         */
        public static IEnumerator RepeatFunction(MonoBehaviour context, Action action, float interval, int count,
            Action onFinish = null)
        {
            if (null == context) throw new ArgumentNullException(nameof(context), "A null context cannot be used for starting coroutines.");
            if (null == action) throw new ArgumentNullException(nameof(action), "Cannot repeat a null action.");
            IEnumerator coroutine = RepeatFunctionWithCountImplementation(action, interval, count, onFinish);
            context.StartCoroutine(coroutine);
            return coroutine;
        }

        /**
         * The internal coroutine implementation used to repeat a function at an interval indefinitely.
         * this can only exit execution by using MonoBehaviour.StopCoroutine(IEnumerator) from the original context.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionIndefiniteImplementation(Action action, float interval)
        {
            while (true)
            {
                yield return new WaitForSeconds(interval);
                action();
            }
        }

        /**
         * The internal coroutine implementation used to repeat a function at an interval a specific number of times
         * this can exit execution early by using MonoBehaviour.StopCoroutine(IEnumerator) from the original context.
         * <param name="action">The action to perform.</param>
         * <param name="interval">The number of seconds to wait between each call to the action.</param>
         * <param name="count">The number of times to repeat the function.</param>
         * <param name="onFinish">An action to be performed after the last action has been performed. Defaults to null.</param>
         * <returns>The coroutine enumerator.</returns>
         */
        private static IEnumerator RepeatFunctionWithCountImplementation(Action action, float interval, int count, Action onFinish)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(interval);
                action();
            }

            if (null != onFinish) onFinish();
        }

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
    }
}
