using OpenAI.Chat;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            return complete;
        }
        catch (Exception)
        {
            return false;
        }
    }
}