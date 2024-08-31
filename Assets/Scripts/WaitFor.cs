using OpenAI.Chat;
using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;

public class WaitFor : CustomYieldInstruction
{
    public override bool keepWaiting => !IsCompleted();

    private Task<ChatResponse> task;

    public WaitFor(Task<ChatResponse> task)
    {
        this.task = task;
    }

    private bool IsCompleted()
    {
        try
        {
            var complete = task.IsCompleted;
            if (complete)
                Debug.Log(task.Result.FirstChoice.Message.Content);
            if (complete && task.Result.FirstChoice.FinishReason != "stop")
                Debug.Log(task.Result.FirstChoice.FinishDetails);
            return complete;
        }
        catch (Exception e)
        {
            Debug.LogError(e);
            throw;
        }
    }

    public static IEnumerator Read(Task<string> task, Func<IEnumerator> callback)
    {
        yield return new WaitUntil(() => task.IsCompleted);

        if (task.IsFaulted)
            yield return callback();
    }
}