using OpenAI;
using OpenAI.Chat;
using System;
using System.Threading.Tasks;

public class ApiKeys
{
    // YOU FOOL, DID YOU REALLY THINK I WOULD LEAVE MY API KEYS IN THE SOURCE CODE?
    // YOU'LL NEVER GET THEM FROM ME, NEVER!
    // insert evil laugh here

    public static string GOOGLE => Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    public static string OPENAI => Environment.GetEnvironmentVariable("OPENAI_API_KEY");
    public static string STREAM => Environment.GetEnvironmentVariable("STREAM_API_KEY");

    public static OpenAIClient API => _api ??= new OpenAIClient(new OpenAIAuthentication(OPENAI));
    private static OpenAIClient _api;
}