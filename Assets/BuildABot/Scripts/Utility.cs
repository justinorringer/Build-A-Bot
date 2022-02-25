using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

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
     * <param name="context">The monobehaviour context used to execute the action.</param>
     * <param name="seconds">The number of seconds to wait before performing the action.</param>
     * <param name="action">The action to perform.</param>
     * <returns>The coroutine enumerator.</returns>
     */
    public static IEnumerator DelayedFunction(MonoBehaviour context, float seconds, Action action)
    {
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
}