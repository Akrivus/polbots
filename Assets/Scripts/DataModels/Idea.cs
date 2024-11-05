public class Idea
{
    public string Prompt { get; set; }
    public string Author { get; set; }
    public string Source { get; set; }

    public Idea(string title, string text, string author, string source)
    {
        if (!string.IsNullOrEmpty(text))
            title = $"{title}: {text}";
        Prompt = title;
        Author = author;
        Source = source;
    }
}