using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class WaitUntilTaskCompleted : CustomYieldInstruction
{
    public override bool keepWaiting => !KeepWaiting();

    private Task task;

    public WaitUntilTaskCompleted(Task task)
    {
        this.task = task;
    }

    private bool KeepWaiting()
    {
        try
        {
            return task.IsCompleted;
        }
        catch
        {
            return true;
        }
    }
}