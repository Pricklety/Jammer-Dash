using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

public class MainThreadDispatcher : MonoBehaviour
{
    private static readonly Queue<Action> actions = new Queue<Action>();
    private static readonly object _lock = new object();

    public static void Enqueue(Action action)
    {
        lock (_lock)
        {
            actions.Enqueue(action);
        }
    }

    public static async Task ExecutePendingActions()
    {
        while (actions.Count > 0)
        {
            Action action;
            lock (_lock)
            {
                action = actions.Dequeue();
            }
            action.Invoke();
            await Task.Yield(); // Ensure we don't lock the main thread
        }
    }
    private void Update()
    {
        lock (actions)
        {
            while (actions.Count > 0)
            {
                Debug.Log("Executing enqueued action.");
                actions.Dequeue().Invoke();
            }
        }
    }

}
