
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class ServerIntegration : MonoBehaviour
{
    public static ServerIntegration Instance => _instance ?? (_instance = FindObjectOfType<ServerIntegration>());
    private static ServerIntegration _instance;

    static Dictionary<string, Dictionary<string, Action<HttpListenerContext>>> routes = new Dictionary<string, Dictionary<string, Action<HttpListenerContext>>>()
    {
        { "GET",    new Dictionary<string, Action<HttpListenerContext>>() },
        { "POST",   new Dictionary<string, Action<HttpListenerContext>>() },
        { "PUT",    new Dictionary<string, Action<HttpListenerContext>>() },
        { "PATCH",  new Dictionary<string, Action<HttpListenerContext>>() },
        { "DELETE", new Dictionary<string, Action<HttpListenerContext>>() },
    };

    private HttpListener listener;
    private Thread thread;

    [SerializeField]
    private ChatGenerator generator;

    public bool IsRunning { get; private set; } = true;

    public void Awake()
    {
        if (_instance != null)
            Debug.LogWarning("Multiple ServerIntegrations found, this is not good.");
        _instance = this;

        AddApiRoute("GET", $"/headlines", async () => await Task.Run(() => ChatManager.Instance.PlayList
            .Select(chat => chat.Headline)
            .ToArray()));
        AddApiRoute<Idea, string>("POST", $"/generate", generator.HandleRequest);
    }

    private void Start()
    {
        listener = new HttpListener();
        listener.Prefixes.Add("http://localhost:8080/");
        thread = new Thread(Listen);
        thread.Start();
    }

    private void OnApplicationQuit()
    {
        listener.Stop();
        IsRunning = false;
    }

    private async void Listen()
    {
        listener.Start();
        while (listener.IsListening && IsRunning)
            ProcessRequest(await listener.GetContextAsync());
        listener.Close();
    }

    private void ProcessRequest(HttpListenerContext context)
    {
        var request = context.Request;
        var response = context.Response;
        response.StatusCode = 200;

        var method = request.HttpMethod;
        var path = request.Url.AbsolutePath;
        var query = request.Url.Query;

        try
        {
            if (routes.ContainsKey(method))
                if (routes[method].ContainsKey(path))
                    routes[method][path](context);
                else
                    response.StatusCode = 404;
            else
                response.StatusCode = 405;
        }
        catch (Exception e)
        {
            var json = JsonConvert.SerializeObject(e);
            response.WriteString(json, "application/json");
            response.StatusCode = 500;
        }
        response.Close();
    }

    public static void Register(string method, string path, Action<HttpListenerContext> handler)
    {
        if (!routes.ContainsKey(method))
            throw new ArgumentException("Invalid method: " + method);
        routes[method][path] = handler;
    }

    public static void AddRoute(string method, string path, Action<HttpListenerContext> handler)
    {
        Register(method, path, handler);
    }

    public static void AddRoute(string method, string path, Func<HttpListenerContext, Task> handler)
    {
        Register(method, path, async context => await handler(context));
    }

    public static void AddRoute(string method, string path, Func<string, Task<string>> handler)
    {
        Register(method, path, async context => await Route(context, handler));
    }

    public static void AddApiRoute<I, O>(string method, string path, Func<I, Task<O>> handler)
    {
        Register(method, path, async context => await ApiRoute(context, handler));
    }

    public static void AddApiRoute<O>(string method, string path, Func<Task<O>> handler)
    {
        Register(method, path, async context => await ApiRoute(context, handler));
    }

    public static void AddGetRoute(string path, Action<Dictionary<string, string>, HttpListenerResponse> handler)
    {
        AddRoute("GET", path, context => GetRoute(context, handler));
    }

    public static async Task Route(HttpListenerContext context, Func<string, Task<string>> route, string contentType = "application/text")
    {
        var req = context.Request;
        var res = context.Response;
        var body = new byte[req.ContentLength64];

        req.InputStream.Read(body, 0, body.Length);

        var text = Encoding.UTF8.GetString(body);
        res.WriteString(await route(text), contentType);
    }

    public async static Task ApiRoute<I, O>(HttpListenerContext context, Func<I, Task<O>> route)
    {
        await Route(context, async text =>
        {
            Debug.Log(text);
            var input = JsonConvert.DeserializeObject<I>(text);
            var output = await route(input);
            return JsonConvert.SerializeObject(output);
        }, "application/json");
    }

    public async static Task ApiRoute<O>(HttpListenerContext context, Func<Task<O>> route)
    {
        await Route(context, async text =>
        {
            var output = await route();
            return JsonConvert.SerializeObject(output);
        }, "application/json");
    }

    public static void GetRoute(HttpListenerContext context, Action<Dictionary<string, string>, HttpListenerResponse> route)
    {
        var req = context.Request;
        var dict = req.Url.Query
            .Substring(1)
            .Split('&')
            .Select(param => param.Split('='))
            .ToDictionary(pair => pair[0], pair => pair[1]);
        route(dict, context.Response);
    }
}