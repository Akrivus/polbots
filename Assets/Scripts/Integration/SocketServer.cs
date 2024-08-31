using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

public class SocketServer : MonoBehaviour
{
    public static SocketServer Instance => _instance ??= FindObjectOfType<SocketServer>();
    private static SocketServer _instance;

    [SerializeField]
    private int port = 22450;

    private TcpListener socket;
    private Thread socketThread;
    private bool run = true;

    public readonly Dictionary<string, Action<TcpClient, string>> Commands = new Dictionary<string, Action<TcpClient, string>>();
    public readonly List<SearchNode> Nodes = new List<SearchNode>();

    public void RespondWith(TcpClient client, string response)
    {
        using var stream = client.GetStream();
        using var writer = new StreamWriter(stream);
        writer.WriteLine(response);
    }

    private void Awake()
    {
        Commands.Add("login", Login);
        Commands.Add("write", Write);
        _instance = this;
    }

    private void Start()
    {
        StoryQueue.Instance.OnQueueAdded += OnQueueAdded;
        StoryQueue.Instance.OnQueueTaken += OnQueueTaken;
        socket = new TcpListener(IPAddress.Any, port);
        socket.Start();
        socketThread = new Thread(() => Listen());
        socketThread.Start();
    }

    private void OnApplicationQuit()
    {
        if (socket != null)
            socket.Stop();
        run = false;
    }

    private void Listen()
    {
        try
        {
            do
            {
                using var client = socket.AcceptTcpClient();
                using var stream = client.GetStream();
                using var reader = new StreamReader(stream);

                var key = reader.ReadLine();
                if (!Commands.ContainsKey(key))
                    continue;
                Commands[key](client, reader.ReadLine());
            } while (run);
        }
        catch (Exception e)
        {
            Debug.LogError(e);
        }
    }

    private void OnQueueAdded(SearchNode node)
    {
        node.Time = DateTime.Now;
        Nodes.Add(node);
    }

    private void OnQueueTaken(SearchNode node)
    {
        Nodes.RemoveAt(0);
    }

    public static void Login(TcpClient client, string data)
    {
        var nodes = JsonConvert.SerializeObject(Instance.Nodes);
        Instance.RespondWith(client, nodes);
    }

    public static void Write(TcpClient client, string data)
    {
        StoryProducer.Instance.ProduceIdea(data);
    }
}