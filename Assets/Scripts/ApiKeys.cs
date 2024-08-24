using OpenAI;
using OpenAI.Chat;
using System;
using System.Threading.Tasks;

public class ApiKeys
{
    public static string GOOGLE => Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    public static string OPENAI => Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    public static string STREAM => Environment.GetEnvironmentVariable("STREAM_API_KEY");

    public static OpenAIClient API => _api ??= new OpenAIClient(new OpenAIAuthentication(OPENAI));
    private static OpenAIClient _api;
}