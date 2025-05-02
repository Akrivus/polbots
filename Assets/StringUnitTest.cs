using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

class StringUnitTest : MonoBehaviour
{
    private void Start()
    {
        var test = "*winks* \"Please! My tacos come with *rich culture*!\" Let's *dance*!";
        var scrubbed = test.Scrub();
        var actions = test.Rinse();

        if (scrubbed != "\"Please! My tacos come with *rich culture*!\"")
            Debug.LogError("Scrub failed!");
        Debug.Log($"Scrubbed: {scrubbed}");

        Debug.Log($"Actions: {string.Join(", ", actions)}");
        if (actions.Length != 3)
            Debug.LogError("Rinse failed!");
    }
}
