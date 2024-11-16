using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using UnityEngine;

public class ManualIntegration : MonoBehaviour
{
    [SerializeField]
    private ChatGenerator ChatGenerator;

    [SerializeField]
    private Idea[] ideas;

    private void Start()
    {
        foreach (var idea in ideas)
            ChatGenerator.AddIdeaToQueue(idea);

    }
}